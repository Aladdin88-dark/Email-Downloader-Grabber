namespace EmailParser
{
    public sealed class AppSettings
    {
        public string InputFile { get; set; } = "urls.txt";
        public string OutputFile { get; set; } = "found_emails.txt";

        public string ImapAccountsFile { get; set; } = "imap_accounts.txt";
        public string ImapServersFile { get; set; } = "imap_servers.txt";
        public string ImapProxyFile { get; set; } = "";
        public string ImapProxyProtocol { get; set; } = "Socks5"; // Socks5 | Http

        // Hotmail/Outlook Web mode inputs
        public string HotmailAccountsFile { get; set; } = "hotmail_accounts.txt";
        public string HotmailProxyFile { get; set; } = "";
        public string HotmailProxyProtocol { get; set; } = "Socks5"; // Socks5 | Http
        public int? MaxParallelHotmailRequests { get; set; }
        public int HotmailDownloadThreads { get; set; } = 50;
        public bool HotmailDownloadEmails { get; set; } = true;
        public bool HotmailDownloadAttachments { get; set; } = false;
        public int HotmailAttachmentMaxSizeKb { get; set; } = 0;
        public string[]? HotmailAttachmentExtensions { get; set; }
        public string[]? HotmailFolders { get; set; }
        public int HotmailFilterMode { get; set; } = 1; // 1=all 2=sender 3=subject 4=sender|subject
        public string[]? HotmailSenderKeywords { get; set; }
        public string[]? HotmailSubjectKeywords { get; set; }
        public bool HotmailScanSeedsAndKeys { get; set; } = true;

        // OneDrive mode inputs
        public string OneDriveAccountsFile { get; set; } = "onedrive_accounts.txt";
        public string OneDriveProxyFile { get; set; } = "";
        public string OneDriveProxyProtocol { get; set; } = "Socks5"; // Socks5 | Http
        public int? MaxParallelOneDriveRequests { get; set; }
        public int OneDriveDownloadThreads { get; set; } = 10;
        public int OneDriveMainMode { get; set; } = 1; // 1=download 2=no download
        public int OneDriveDownloadSubMode { get; set; } = 1; // 1=ext 2=kw 3=both 4=api
        public string[]? OneDriveFilterList { get; set; }
        public int OneDriveMaxSizeKb { get; set; } = 0;
        public bool OneDriveScanSeedsAndKeys { get; set; } = true;

        // Photo seed/private key mode inputs.
        public string CryptoKeyPhotoSourcePath { get; set; } = "";
        public int CryptoKeyWorkers { get; set; } = 4;

        // Anti-Public photo feature analysis.
        public string AntiPublicPhotoSourcePath { get; set; } = "";
        public int AntiPublicWorkers { get; set; } = 4;

        // Upload mode can have its own independent IMAP inputs.
        // If not set in appsettings.json, SettingsStore.Load() will fall back to Imap* values.
        public string? UploadImapAccountsFile { get; set; }
        public string? UploadImapProxyFile { get; set; }
        public string? UploadImapProxyProtocol { get; set; } // Socks5 | Http

        // Attachment filter for Upload mode.
        // If UploadAttachmentAll is true OR UploadAttachmentExtensions is empty => download any attachment.
        public bool UploadAttachmentAll { get; set; } = true;
        public string[]? UploadAttachmentExtensions { get; set; }

        // Email filter for Upload mode.
        // 1=all 2=sender 3=subject 4=sender|subject
        public int UploadFilterMode { get; set; } = 1;
        public bool UploadDownloadAttachments { get; set; } = true;
        public bool UploadDownloadEmails { get; set; } = false;
        public int UploadEmailFromDay { get; set; } = 1;
        public int UploadEmailFromMonth { get; set; } = 1;
        public int UploadEmailFromYear { get; set; } = 2000;
        public string[]? UploadSenderKeywords { get; set; }
        public string[]? UploadSubjectKeywords { get; set; }

        // Recheck: if enabled, failed IMAP accounts are retried later (pushed to end of queue).
        // Delay fields are kept for compatibility but are not used in the queue-based recheck.
        public bool MailRecheckEnabled { get; set; } = false;
        public int MailRecheckDelaySeconds { get; set; } = 0;
        public bool UploadRecheckEnabled { get; set; } = false;
        public int UploadRecheckDelaySeconds { get; set; } = 0;

        // Upload mode seed/key scanning.
        public bool UploadScanSeedsAndKeys { get; set; } = false;

        public int DelayMs { get; set; } = 500;
        // Web mode threads.
        public int MaxParallelRequests { get; set; } = 4;

        // Mail/Upload can be configured independently.
        public int? MaxParallelMailRequests { get; set; }
        public int? MaxParallelUploadRequests { get; set; }
        public int RequestTimeoutSeconds { get; set; } = 10;
        public string UserAgent { get; set; } = "EmailParser/1.0";

        public bool SearchInScripts { get; set; } = true;

        // Если true — страница будет рендериться WebKit (JS выполнится),
        // после чего будет взят итоговый HTML (page.ContentAsync).
        public bool EnableJavaScriptRendering { get; set; } = false;
        public int JsNavigationTimeoutSeconds { get; set; } = 20;
        public int JsWaitAfterLoadMs { get; set; } = 0;
    }
}
