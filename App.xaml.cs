using ModernWpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Auto_Uploader;

namespace EmailParser
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            RegisterCrashLogging(this);

            // По умолчанию включаем тёмную тему, чтобы UI не выглядел "всё белое".
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;

            // Только теперь запускаем главное окно
            var mainWindow = new MainWindow();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private static void RegisterCrashLogging(Application app)
        {
            void WriteCrash(Exception ex)
            {
                try
                {
                    var dir = Path.Combine(AppContext.BaseDirectory, "Result");
                    Directory.CreateDirectory(dir);
                    var path = Path.Combine(dir, "crash.log");
                    File.AppendAllText(path, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\n{ex}\n\n");
                }
                catch
                {
                    // ignore
                }
            }

            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                    WriteCrash(ex);
            };

            app.DispatcherUnhandledException += (_, args) =>
            {
                WriteCrash(args.Exception);
                // Let it crash after logging (so we don't keep app in broken state)
                args.Handled = false;
            };

            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                WriteCrash(args.Exception);
            };
        }
    }
}
