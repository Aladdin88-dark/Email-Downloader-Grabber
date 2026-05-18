using System;
using System.IO;
using System.Text.Json;

namespace EmailParser
{
    public static class SettingsStore
    {
        public const string SettingsFileName = "appsettings.json";

        public static AppSettings Load()
        {
            if (!File.Exists(SettingsFileName))
                return Normalize(new AppSettings());

            try
            {
                var json = File.ReadAllText(SettingsFileName);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return Normalize(settings ?? new AppSettings());
            }
            catch
            {
                return Normalize(new AppSettings());
            }
        }

        private static AppSettings Normalize(AppSettings s)
        {
            // Threads: if Mail/Upload not specified, default to web threads.
            s.MaxParallelMailRequests ??= s.MaxParallelRequests;
            s.MaxParallelUploadRequests ??= s.MaxParallelRequests;
            s.MaxParallelHotmailRequests ??= s.MaxParallelRequests;

            // Upload IMAP settings fall back to Mail IMAP settings when not specified.
            if (string.IsNullOrWhiteSpace(s.UploadImapAccountsFile))
                s.UploadImapAccountsFile = s.ImapAccountsFile;

            if (string.IsNullOrWhiteSpace(s.UploadImapProxyFile))
                s.UploadImapProxyFile = s.ImapProxyFile;

            if (string.IsNullOrWhiteSpace(s.UploadImapProxyProtocol))
                s.UploadImapProxyProtocol = s.ImapProxyProtocol;

            // Hotmail falls back to main IMAP inputs when not specified.
            if (string.IsNullOrWhiteSpace(s.HotmailAccountsFile))
                s.HotmailAccountsFile = s.ImapAccountsFile;

            if (string.IsNullOrWhiteSpace(s.HotmailProxyFile))
                s.HotmailProxyFile = s.ImapProxyFile;

            if (string.IsNullOrWhiteSpace(s.HotmailProxyProtocol))
                s.HotmailProxyProtocol = s.ImapProxyProtocol;

            // Attachment filter defaults.
            // Keep UploadAttachmentAll defaulting to true, but if user explicitly set extensions and set All=false,
            // we respect that. If extensions is null/empty, treat as All.
            if (s.UploadAttachmentExtensions != null && s.UploadAttachmentExtensions.Length == 0)
                s.UploadAttachmentExtensions = null;

            if (s.UploadAttachmentExtensions == null)
                s.UploadAttachmentAll = true;

            if (s.UploadScanSeedsAndKeys)
                s.UploadDownloadAttachments = true;

            if (s.HotmailAttachmentExtensions != null && s.HotmailAttachmentExtensions.Length == 0)
                s.HotmailAttachmentExtensions = null;

            if (s.HotmailFolders == null || s.HotmailFolders.Length == 0)
                s.HotmailFolders = new[] { "inbox", "sent", "drafts" };

            // OneDrive falls back to Hotmail inputs (both are web-based auth).
            if (string.IsNullOrWhiteSpace(s.OneDriveAccountsFile))
                s.OneDriveAccountsFile = s.HotmailAccountsFile;

            if (string.IsNullOrWhiteSpace(s.OneDriveProxyFile))
                s.OneDriveProxyFile = s.HotmailProxyFile;

            if (string.IsNullOrWhiteSpace(s.OneDriveProxyProtocol))
                s.OneDriveProxyProtocol = s.HotmailProxyProtocol;

            s.MaxParallelOneDriveRequests ??= s.MaxParallelRequests;

            // Recheck delay bounds (kept for settings compatibility).
            if (s.MailRecheckDelaySeconds < 0) s.MailRecheckDelaySeconds = 0;
            if (s.MailRecheckDelaySeconds > 3600) s.MailRecheckDelaySeconds = 3600;
            if (s.UploadRecheckDelaySeconds < 0) s.UploadRecheckDelaySeconds = 0;
            if (s.UploadRecheckDelaySeconds > 3600) s.UploadRecheckDelaySeconds = 3600;

            if (s.CryptoKeyWorkers < 1) s.CryptoKeyWorkers = 4;
            if (s.CryptoKeyWorkers > 64) s.CryptoKeyWorkers = 64;

            if (s.AntiPublicWorkers < 1) s.AntiPublicWorkers = 4;
            if (s.AntiPublicWorkers > 16) s.AntiPublicWorkers = 16;


            return s;
        }

        public static void Save(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(SettingsFileName, json);
        }

        public static string EnsureAbsolutePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Путь не задан.");

            return Path.GetFullPath(path);
        }
    }
}
