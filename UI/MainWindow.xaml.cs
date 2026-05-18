using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Reflection;

namespace EmailParser
{
    public partial class MainWindow : Window
    {
        private enum AppMode
        {
            Checker,
            Hotmail,
            Upload,
            OneDrive,
            CryptoKey,
            AntiPublic
        }

        private CancellationTokenSource? _cts;
        private AppMode _mode = AppMode.Checker;
        private string? _lastHotmailDir;

        public MainWindow()
        {
            InitializeComponent();
            LoadUiFromSettings();

            CheckerTab.IsChecked    = true;
            HotmailTab.IsChecked    = false;
            UploadTab.IsChecked     = false;
            OneDriveTab.IsChecked   = false;
            CryptoKeyTab.IsChecked  = false;
            AntiPublicTab.IsChecked = false;
            SetMode(AppMode.Checker);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try { Directory.CreateDirectory(GetResultRootDir()); } catch { }
            await Task.CompletedTask;
        }

        private void LoadUiFromSettings()
        {
            var settings = SettingsStore.Load();

            MaxParallelUploadText.Value  = Math.Min(settings.MaxParallelUploadRequests ?? settings.MaxParallelRequests, 50);
            HotmailMaxParallelText.Value = Math.Min(settings.MaxParallelHotmailRequests ?? settings.MaxParallelRequests, 50);

            CheckerImapAccountsFileText.Text = settings.ImapAccountsFile;
            CheckerImapProxyFileText.Text     = settings.ImapProxyFile;

            UploadImapAccountsFileText.Text = settings.UploadImapAccountsFile ?? settings.ImapAccountsFile;
            UploadImapProxyFileText.Text     = settings.UploadImapProxyFile   ?? settings.ImapProxyFile;

            HotmailAccountsFileText.Text = settings.HotmailAccountsFile;
            HotmailProxyFileText.Text     = settings.HotmailProxyFile;

            var protoUpload  = (settings.UploadImapProxyProtocol ?? settings.ImapProxyProtocol ?? "Socks5").Trim();
            UploadImapProxyProtocolCombo.SelectedIndex = protoUpload.Equals("http", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            var protoHotmail = (settings.HotmailProxyProtocol ?? "Socks5").Trim();
            HotmailProxyProtocolCombo.SelectedIndex = protoHotmail.Equals("http", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            // Hidden fields needed to avoid null refs on BuildSettingsFromUi
            CheckerMaxParallelText.Value       = settings.MaxParallelMailRequests ?? settings.MaxParallelRequests;
            CheckerImapProxyProtocolCombo.SelectedIndex = 0;

            HotmailDownloadThreadsText.Value   = settings.HotmailDownloadThreads;
            HotmailDownloadEmailsChk.IsChecked      = false;
            HotmailDownloadAttachmentsChk.IsChecked = false;
            HotmailScanSeedsChk.IsChecked           = false;
            HotmailAttachmentMaxSizeText.Value = settings.HotmailAttachmentMaxSizeKb;
            HotmailAttachmentExtensionsText.Text = string.Empty;

            HotmailFolderInboxChk.IsChecked   = false;
            HotmailFolderSentChk.IsChecked    = false;
            HotmailFolderDraftsChk.IsChecked  = false;

            HotmailFilterModeCombo.SelectedIndex = 0;
            HotmailSenderKeywordsText.Text  = string.Empty;
            HotmailSubjectKeywordsText.Text = string.Empty;

            UploadScanSeedsChk.IsChecked           = false;
            UploadDownloadAttachmentsChk.IsChecked = false;
            UploadDownloadEmailsChk.IsChecked       = false;
            UploadFilterModeCombo.SelectedIndex     = 0;
            UploadEmailFromDayText.Value   = 1;
            UploadEmailFromMonthText.Value = 1;
            UploadEmailFromYearText.Value  = 2000;
            UploadSenderKeywordsText.Text  = string.Empty;
            UploadSubjectKeywordsText.Text = string.Empty;
            UploadExtAllChk.IsChecked      = true;

            OneDriveAccountsFileText.Text = settings.OneDriveAccountsFile;
            OneDriveProxyFileText.Text     = settings.OneDriveProxyFile;
            OneDriveProxyProtocolCombo.SelectedIndex = 0;
            OneDriveMaxParallelText.Value   = settings.MaxParallelOneDriveRequests ?? settings.MaxParallelRequests;
            OneDriveDownloadThreadsText.Value = settings.OneDriveDownloadThreads;
            OneDriveMainModeCombo.SelectedIndex = Math.Max(0, settings.OneDriveMainMode - 1);
            OneDriveSubModeCombo.SelectedIndex  = Math.Max(0, settings.OneDriveDownloadSubMode - 1);
            OneDriveFilterText.Text = string.Empty;
            OneDriveMaxSizeText.Value = settings.OneDriveMaxSizeKb;
            OneDriveScanSeedsChk.IsChecked = false;

            CryptoKeySourcePathText.Text  = settings.CryptoKeyPhotoSourcePath;
            CryptoKeyWorkersText.Value    = settings.CryptoKeyWorkers;

            AntiPublicSourcePathText.Text = settings.AntiPublicPhotoSourcePath;
            AntiPublicWorkersText.Value   = settings.AntiPublicWorkers;
        }

        private void SaveSettingsFromUi()
        {
            var settings = BuildSettingsFromUi();
            SettingsStore.Save(settings);
        }

        private AppSettings BuildSettingsFromUi()
        {
            var settings = SettingsStore.Load();

            settings.MaxParallelUploadRequests  = ClampInt(MaxParallelUploadText.Value,  settings.MaxParallelRequests, 1, 50);
            settings.MaxParallelHotmailRequests = ClampInt(HotmailMaxParallelText.Value, settings.MaxParallelRequests, 1, 50);

            settings.ImapAccountsFile    = (CheckerImapAccountsFileText.Text ?? "").Trim();
            settings.ImapProxyFile       = (CheckerImapProxyFileText.Text     ?? "").Trim();
            settings.ImapProxyProtocol   = "Socks5";

            settings.UploadImapAccountsFile  = (UploadImapAccountsFileText.Text ?? "").Trim();
            settings.UploadImapProxyFile     = (UploadImapProxyFileText.Text    ?? "").Trim();
            settings.UploadImapProxyProtocol = UploadImapProxyProtocolCombo.SelectedIndex == 1 ? "Http" : "Socks5";

            settings.HotmailAccountsFile  = (HotmailAccountsFileText.Text ?? "").Trim();
            settings.HotmailProxyFile     = (HotmailProxyFileText.Text    ?? "").Trim();
            settings.HotmailProxyProtocol = HotmailProxyProtocolCombo.SelectedIndex == 1 ? "Http" : "Socks5";

            settings.HotmailDownloadEmails      = false;
            settings.HotmailDownloadAttachments = false;
            settings.HotmailScanSeedsAndKeys    = false;
            settings.UploadScanSeedsAndKeys     = false;
            settings.UploadDownloadAttachments  = false;
            settings.UploadDownloadEmails       = false;

            settings.OneDriveAccountsFile  = (OneDriveAccountsFileText.Text ?? "").Trim();
            settings.OneDriveProxyFile     = (OneDriveProxyFileText.Text    ?? "").Trim();
            settings.OneDriveProxyProtocol = "Socks5";

            settings.CryptoKeyPhotoSourcePath  = (CryptoKeySourcePathText.Text  ?? "").Trim();
            settings.AntiPublicPhotoSourcePath = (AntiPublicSourcePathText.Text ?? "").Trim();

            return settings;
        }

        private static int ClampInt(double? value, int fallback, int min, int max)
        {
            if (!value.HasValue) return fallback;
            var intValue = (int)Math.Round(value.Value);
            if (intValue < min) return min;
            if (intValue > max) return max;
            return intValue;
        }

        private static string[]? ParseCsvList(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var parts = raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim()).Where(p => p.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            return parts.Length == 0 ? null : parts;
        }

        private static string JoinCsvList(string[]? items)
            => items == null || items.Length == 0 ? string.Empty : string.Join(", ", items);

        private void AppendLog(string message)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (LogBox.Text.Length > 200000)
                    LogBox.Clear();
                LogBox.AppendText(message + Environment.NewLine);
                LogBox.ScrollToEnd();
            }, DispatcherPriority.Background);
        }

        private void SetStatus(string text, double? progress = null)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = text;
                if (progress.HasValue)
                    Progress.Value = Math.Max(0, Math.Min(100, progress.Value));
            });
        }

        private void SetRunningUi(bool running)
        {
            var canRun = _mode == AppMode.Hotmail || _mode == AppMode.Upload;
            StartBtn.IsEnabled  = !running && canRun;
            CancelBtn.IsEnabled = running;
            OpenOutputBtn.IsEnabled = _mode == AppMode.Hotmail || _mode == AppMode.Upload;

            if (_mode == AppMode.Upload)
            {
                BrowseUploadImapAccountsBtn.IsEnabled = !running;
                BrowseUploadImapProxyBtn.IsEnabled    = !running;
                UploadImapProxyProtocolCombo.IsEnabled = !running;
                MaxParallelUploadText.IsEnabled        = !running;
            }
            else if (_mode == AppMode.Hotmail)
            {
                BrowseHotmailAccountsBtn.IsEnabled  = !running;
                BrowseHotmailProxyBtn.IsEnabled     = !running;
                HotmailProxyProtocolCombo.IsEnabled = !running;
                HotmailMaxParallelText.IsEnabled    = !running;
            }
        }

        private void CheckerTab_Checked(object sender, RoutedEventArgs e)
        {
            if (_mode != AppMode.Checker) SetMode(AppMode.Checker);
        }

        private void UploadTab_Checked(object sender, RoutedEventArgs e)
        {
            if (_mode != AppMode.Upload) SetMode(AppMode.Upload);
        }

        private void HotmailTab_Checked(object sender, RoutedEventArgs e)
        {
            if (_mode != AppMode.Hotmail) SetMode(AppMode.Hotmail);
        }

        private void OneDriveTab_Checked(object sender, RoutedEventArgs e)
        {
            if (_mode != AppMode.OneDrive) SetMode(AppMode.OneDrive);
        }

        private void CryptoKeyTab_Checked(object sender, RoutedEventArgs e)
        {
            if (_mode != AppMode.CryptoKey) SetMode(AppMode.CryptoKey);
        }

        private void AntiPublicTab_Checked(object sender, RoutedEventArgs e)
        {
            if (_mode != AppMode.AntiPublic) SetMode(AppMode.AntiPublic);
        }

        private void SetMode(AppMode mode)
        {
            _mode = mode;

            if (mode == AppMode.Checker)
            {
                if (CheckerTab.IsChecked    != true)  CheckerTab.IsChecked    = true;
                if (HotmailTab.IsChecked    != false) HotmailTab.IsChecked    = false;
                if (UploadTab.IsChecked     != false) UploadTab.IsChecked     = false;
                if (OneDriveTab.IsChecked   != false) OneDriveTab.IsChecked   = false;
                if (CryptoKeyTab.IsChecked  != false) CryptoKeyTab.IsChecked  = false;
                if (AntiPublicTab.IsChecked != false) AntiPublicTab.IsChecked = false;
            }
            else if (mode == AppMode.Hotmail)
            {
                if (HotmailTab.IsChecked    != true)  HotmailTab.IsChecked    = true;
                if (CheckerTab.IsChecked    != false) CheckerTab.IsChecked    = false;
                if (UploadTab.IsChecked     != false) UploadTab.IsChecked     = false;
                if (OneDriveTab.IsChecked   != false) OneDriveTab.IsChecked   = false;
                if (CryptoKeyTab.IsChecked  != false) CryptoKeyTab.IsChecked  = false;
                if (AntiPublicTab.IsChecked != false) AntiPublicTab.IsChecked = false;
            }
            else if (mode == AppMode.Upload)
            {
                if (UploadTab.IsChecked     != true)  UploadTab.IsChecked     = true;
                if (CheckerTab.IsChecked    != false) CheckerTab.IsChecked    = false;
                if (HotmailTab.IsChecked    != false) HotmailTab.IsChecked    = false;
                if (OneDriveTab.IsChecked   != false) OneDriveTab.IsChecked   = false;
                if (CryptoKeyTab.IsChecked  != false) CryptoKeyTab.IsChecked  = false;
                if (AntiPublicTab.IsChecked != false) AntiPublicTab.IsChecked = false;
            }
            else if (mode == AppMode.OneDrive)
            {
                if (OneDriveTab.IsChecked   != true)  OneDriveTab.IsChecked   = true;
                if (CheckerTab.IsChecked    != false) CheckerTab.IsChecked    = false;
                if (HotmailTab.IsChecked    != false) HotmailTab.IsChecked    = false;
                if (UploadTab.IsChecked     != false) UploadTab.IsChecked     = false;
                if (CryptoKeyTab.IsChecked  != false) CryptoKeyTab.IsChecked  = false;
                if (AntiPublicTab.IsChecked != false) AntiPublicTab.IsChecked = false;
            }
            else if (mode == AppMode.CryptoKey)
            {
                if (CryptoKeyTab.IsChecked  != true)  CryptoKeyTab.IsChecked  = true;
                if (CheckerTab.IsChecked    != false) CheckerTab.IsChecked    = false;
                if (HotmailTab.IsChecked    != false) HotmailTab.IsChecked    = false;
                if (UploadTab.IsChecked     != false) UploadTab.IsChecked     = false;
                if (OneDriveTab.IsChecked   != false) OneDriveTab.IsChecked   = false;
                if (AntiPublicTab.IsChecked != false) AntiPublicTab.IsChecked = false;
            }
            else
            {
                if (AntiPublicTab.IsChecked != true)  AntiPublicTab.IsChecked = true;
                if (CheckerTab.IsChecked    != false) CheckerTab.IsChecked    = false;
                if (HotmailTab.IsChecked    != false) HotmailTab.IsChecked    = false;
                if (UploadTab.IsChecked     != false) UploadTab.IsChecked     = false;
                if (OneDriveTab.IsChecked   != false) OneDriveTab.IsChecked   = false;
                if (CryptoKeyTab.IsChecked  != false) CryptoKeyTab.IsChecked  = false;
            }

            CheckerPanel.Visibility    = mode == AppMode.Checker    ? Visibility.Visible : Visibility.Collapsed;
            UploadPanel.Visibility     = mode == AppMode.Upload     ? Visibility.Visible : Visibility.Collapsed;
            HotmailPanel.Visibility    = mode == AppMode.Hotmail    ? Visibility.Visible : Visibility.Collapsed;
            OneDrivePanel.Visibility   = mode == AppMode.OneDrive   ? Visibility.Visible : Visibility.Collapsed;
            CryptoKeyPanel.Visibility  = mode == AppMode.CryptoKey  ? Visibility.Visible : Visibility.Collapsed;
            AntiPublicPanel.Visibility = mode == AppMode.AntiPublic ? Visibility.Visible : Visibility.Collapsed;

            if (mode == AppMode.Checker)
            {
                Title = "Profile";
                AppTitleText.Text    = "Email Grabber DEMO";
                AppSubtitleText.Text = "Free open-source demo — valid check only.";
                StartBtnLabel.Text   = "Start";
            }
            else if (mode == AppMode.Hotmail)
            {
                Title = "Hotmail";
                AppTitleText.Text    = "Hotmail";
                AppSubtitleText.Text = "Outlook Web API: check accounts (valid check only).";
                StartBtnLabel.Text   = "Start";
            }
            else if (mode == AppMode.Upload)
            {
                Title = "IMAP";
                AppTitleText.Text    = "IMAP";
                AppSubtitleText.Text = "IMAP: check account validity. Results saved to Result/Valid_Email.txt.";
                StartBtnLabel.Text   = "Start";
            }
            else if (mode == AppMode.OneDrive)
            {
                Title = "OneDrive";
                AppTitleText.Text    = "OneDrive";
                AppSubtitleText.Text = "Not available in DEMO version.";
                StartBtnLabel.Text   = "Start";
            }
            else if (mode == AppMode.CryptoKey)
            {
                Title = "Crypto Key";
                AppTitleText.Text    = "Crypto Key";
                AppSubtitleText.Text = "Not available in DEMO version.";
                StartBtnLabel.Text   = "Start";
            }
            else
            {
                Title = "Anti-Public";
                AppTitleText.Text    = "Anti-Public";
                AppSubtitleText.Text = "Not available in DEMO version.";
                StartBtnLabel.Text   = "Start";
            }

            FooterNoteText.Text = mode == AppMode.Hotmail
                ? "Note: Hotmail uses Outlook Web API. Valid check only."
                : mode == AppMode.Upload
                    ? "Note: IMAP servers are loaded from imap_servers.txt next to the app."
                    : mode == AppMode.Checker
                        ? "Note: DEMO version — valid check for IMAP (M) and Hotmail (H)."
                        : "Note: This feature is not available in the DEMO version.";

            SetRunningUi(_cts != null);
        }

        private void BrowseImapAccountsBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title  = "Select IMAP accounts file",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = CheckerImapAccountsFileText.Text
            };
            if (dlg.ShowDialog(this) == true)
            {
                CheckerImapAccountsFileText.Text = dlg.FileName;
                SaveSettingsFromUi();
            }
        }

        private void BrowseImapProxyBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title  = "Select proxy file",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = CheckerImapProxyFileText.Text
            };
            if (dlg.ShowDialog(this) == true)
            {
                CheckerImapProxyFileText.Text = dlg.FileName;
                SaveSettingsFromUi();
            }
        }

        private void BrowseUploadImapAccountsBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title  = "Select IMAP accounts file",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = UploadImapAccountsFileText.Text
            };
            if (dlg.ShowDialog(this) == true)
            {
                UploadImapAccountsFileText.Text = dlg.FileName;
                SaveSettingsFromUi();
            }
        }

        private void BrowseUploadImapProxyBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title  = "Select proxy file",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = UploadImapProxyFileText.Text
            };
            if (dlg.ShowDialog(this) == true)
            {
                UploadImapProxyFileText.Text = dlg.FileName;
                SaveSettingsFromUi();
            }
        }

        private void BrowseHotmailAccountsBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title  = "Select Hotmail accounts file",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = HotmailAccountsFileText.Text
            };
            if (dlg.ShowDialog(this) == true)
            {
                HotmailAccountsFileText.Text = dlg.FileName;
                SaveSettingsFromUi();
            }
        }

        private void BrowseHotmailProxyBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title  = "Select proxy file",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = HotmailProxyFileText.Text
            };
            if (dlg.ShowDialog(this) == true)
            {
                HotmailProxyFileText.Text = dlg.FileName;
                SaveSettingsFromUi();
            }
        }

        private void BrowseOneDriveAccountsBtn_Click(object sender, RoutedEventArgs e) { }
        private void BrowseOneDriveProxyBtn_Click(object sender, RoutedEventArgs e) { }
        private void BrowseCryptoKeySourceBtn_Click(object sender, RoutedEventArgs e) { }
        private void CryptoKeyInput_LostFocus(object sender, RoutedEventArgs e) { }
        private void BrowseAntiPublicSourceBtn_Click(object sender, RoutedEventArgs e) { }
        private void AntiPublicInput_LostFocus(object sender, RoutedEventArgs e) { }

        private void HotmailDownloadAttachmentsChk_Checked(object sender, RoutedEventArgs e) { }
        private void HotmailDownloadAttachmentsChk_Unchecked(object sender, RoutedEventArgs e) { }
        private void HotmailDownloadEmailsChk_Checked(object sender, RoutedEventArgs e) { }
        private void HotmailDownloadEmailsChk_Unchecked(object sender, RoutedEventArgs e) { }

        private void UploadExtAllChk_Checked(object sender, RoutedEventArgs e) { }
        private void UploadExtAllChk_Unchecked(object sender, RoutedEventArgs e) { }
        private void UploadScanSeedsChk_Changed(object sender, RoutedEventArgs e) { }
        private void UploadDownloadAttachmentsChk_Changed(object sender, RoutedEventArgs e) { }
        private void UploadDownloadEmailsChk_Changed(object sender, RoutedEventArgs e) { }
        private void UploadExtSpecificChk_Checked(object sender, RoutedEventArgs e) { }

        private async void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_mode == AppMode.Checker || _mode == AppMode.OneDrive ||
                _mode == AppMode.CryptoKey || _mode == AppMode.AntiPublic)
                return;

            LogBox.Clear();
            _lastHotmailDir = null;

            AppSettings settings;
            try
            {
                settings = BuildSettingsFromUi();
                SettingsStore.Save(settings);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_mode == AppMode.Upload)
            {
                var uploadAccounts = (settings.UploadImapAccountsFile ?? "").Trim();
                if (string.IsNullOrWhiteSpace(uploadAccounts) || !File.Exists(uploadAccounts))
                {
                    MessageBox.Show(this, $"IMAP accounts file not found: {uploadAccounts}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else if (_mode == AppMode.Hotmail)
            {
                var hotmailPath = (HotmailAccountsFileText.Text ?? "").Trim();
                var checkerPath = (CheckerImapAccountsFileText.Text ?? "").Trim();
                var effectivePath = string.IsNullOrWhiteSpace(hotmailPath) ? checkerPath : hotmailPath;
                if (string.IsNullOrWhiteSpace(effectivePath) || !File.Exists(effectivePath))
                {
                    MessageBox.Show(this, $"Hotmail accounts file not found: {effectivePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            _cts = new CancellationTokenSource();
            SetRunningUi(true);
            SetStatus("Starting…", 0);

            try
            {
                if (_mode == AppMode.Upload)
                {
                    // M mode: IMAP valid check using EmailChecker
                    var effectiveSettings = new AppSettings
                    {
                        ImapAccountsFile    = settings.UploadImapAccountsFile ?? settings.ImapAccountsFile,
                        ImapServersFile     = settings.ImapServersFile,
                        ImapProxyFile       = settings.UploadImapProxyFile   ?? settings.ImapProxyFile,
                        ImapProxyProtocol   = settings.UploadImapProxyProtocol ?? settings.ImapProxyProtocol,
                        MaxParallelRequests = settings.MaxParallelUploadRequests ?? settings.MaxParallelRequests,
                        RequestTimeoutSeconds = settings.RequestTimeoutSeconds,
                        MailRecheckEnabled  = false
                    };

                    var checker = new EmailChecker(effectiveSettings, AppendLog);
                    await checker.RunAsync(_cts.Token, progress =>
                    {
                        SetStatus(progress.Status, progress.Percent);
                    });
                    SetStatus("Done", 100);
                }
                else if (_mode == AppMode.Hotmail)
                {
                    var effectiveSettings = new AppSettings
                    {
                        HotmailAccountsFile       = settings.HotmailAccountsFile,
                        HotmailProxyFile          = settings.HotmailProxyFile,
                        HotmailProxyProtocol      = settings.HotmailProxyProtocol,
                        MaxParallelHotmailRequests= settings.MaxParallelHotmailRequests,
                        MaxParallelRequests       = settings.MaxParallelRequests,
                        ImapAccountsFile          = settings.ImapAccountsFile,
                        ImapProxyFile             = settings.ImapProxyFile,
                        HotmailDownloadEmails     = false,
                        HotmailDownloadAttachments= false,
                        HotmailScanSeedsAndKeys   = false,
                        RequestTimeoutSeconds     = settings.RequestTimeoutSeconds
                    };

                    var hotmail = new HotmailProcessor(effectiveSettings, AppendLog);
                    await hotmail.RunAsync(_cts.Token, progress =>
                    {
                        SetStatus(progress.Status, progress.Percent);
                    });
                    _lastHotmailDir = hotmail.ResultDir;
                    SetStatus("Done", 100);
                }
            }
            catch (OperationCanceledException)
            {
                SetStatus("Stopped", 0);
                AppendLog("Operation cancelled.");
            }
            catch (Exception ex)
            {
                SetStatus("Error", 0);
                AppendLog("Error: " + ex.Message);
                MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                SetRunningUi(false);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }

        private void OpenOutputBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_mode == AppMode.Hotmail)
                {
                    var dir = _lastHotmailDir;
                    if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                        dir = FindLatestHotmailResultDir();

                    if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                    {
                        MessageBox.Show(this, "Result folder not created yet.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true });
                }
                else if (_mode == AppMode.Upload)
                {
                    // M mode opens Valid_Email.txt
                    var path = EmailChecker.GetDefaultOutputPath();
                    if (!File.Exists(path))
                    {
                        MessageBox.Show(this, "Result file not created yet.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GetResultRootDir() =>
            Path.Combine(GetAppBaseDir(), "Result");

        private static string GetAppBaseDir()
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
            return baseDir;
        }

        private static string? FindLatestHotmailResultDir()
        {
            var root = GetResultRootDir();
            if (!Directory.Exists(root)) return null;
            var dirs = Directory.GetDirectories(root, "Hotmail_*");
            if (dirs.Length == 0) return null;
            return dirs.Select(d => new DirectoryInfo(d))
                .OrderByDescending(d => d.CreationTimeUtc)
                .FirstOrDefault()?.FullName;
        }

        private static string GetProductVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version == null) return "3.0";
            return version.Build > 0
                ? $"{version.Major}.{version.Minor}.{version.Build}"
                : $"{version.Major}.{version.Minor}";
        }

        // Unused UI references required to avoid XAML binding errors — kept as no-ops
        private void ApplyUploadExtensionFilterUiRules() { }
        private void UpdateUploadDownloadControlAvailability(bool running) { }
        private System.Collections.Generic.List<string> GetSelectedUploadExtensions() => new();
        private string[] GetSelectedHotmailFolders() => Array.Empty<string>();
    }
}
