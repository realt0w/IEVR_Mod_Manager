using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace IEVRModManager
{
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            EnsureBaseDirectory();
            ApplyLanguage();
            ApplyTheme();
            StartupLog.Log("App starting.");
            base.OnStartup(e);
        }

        private void ApplyLanguage()
        {
            try
            {
                // Load config to get language preference
                var configManager = new Managers.ConfigManager();
                var config = configManager.Load();
                string language = string.IsNullOrWhiteSpace(config.Language) ? "System" : config.Language;
                StartupLog.Log($"Applying language: {language}");
                Helpers.LocalizationHelper.SetLanguage(language);
                
                // Test that strings are loading
                var testString = Helpers.LocalizationHelper.GetString("AppTitle");
                StartupLog.Log($"Test string 'AppTitle' = '{testString}'");
            }
            catch (Exception ex)
            {
                StartupLog.Log("Error applying language", ex);
            }
        }

        private void ApplyTheme()
        {
            try
            {
                // Load config to get theme preference
                var configManager = new Managers.ConfigManager();
                var config = configManager.Load();
                string theme = string.IsNullOrWhiteSpace(config.Theme) ? "System" : config.Theme;

                // Determine which theme to use
                string themeToUse;
                if (theme == "System")
                {
                    // Detect system theme
                    themeToUse = IsSystemThemeDark() ? "Dark" : "Light";
                }
                else
                {
                    themeToUse = theme;
                }

                // Apply theme
                var resources = Current.Resources;
                resources.MergedDictionaries.Clear();

                var themeDict = new ResourceDictionary();
                if (themeToUse == "Light")
                {
                    themeDict.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
                }
                else
                {
                    themeDict.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
                }
                resources.MergedDictionaries.Add(themeDict);
            }
            catch (Exception ex)
            {
                StartupLog.Log("Error applying theme", ex);
                // Fallback to dark theme
                var resources = Current.Resources;
                resources.MergedDictionaries.Clear();
                var themeDict = new ResourceDictionary();
                themeDict.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
                resources.MergedDictionaries.Add(themeDict);
            }
        }

        private bool IsSystemThemeDark()
        {
            try
            {
                // Check Windows registry for theme preference
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        var appsUseLightTheme = key.GetValue("AppsUseLightTheme");
                        if (appsUseLightTheme != null && appsUseLightTheme is int value)
                        {
                            return value == 0; // 0 = dark theme, 1 = light theme
                        }
                    }
                }
            }
            catch
            {
                // If registry access fails, default to dark theme
            }
            return true; // Default to dark theme
        }


        private static void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            StartupLog.Log("Dispatcher exception", e.Exception);
            MessageBox.Show($"Unhandled error:\n{e.Exception.Message}", "IEVR Mod Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            StartupLog.Log("Domain unhandled exception", e.ExceptionObject as Exception);
            MessageBox.Show("Unhandled error. See app.log in AppData.", "IEVR Mod Manager", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void OnUnobservedTaskException(object? sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            StartupLog.Log("Task unobserved exception", e.Exception);
            e.SetObserved();
        }

        private static void EnsureBaseDirectory()
        {
            try
            {
                Directory.CreateDirectory(Config.BaseDir);
                Directory.SetCurrentDirectory(Config.BaseDir);
            }
            catch (Exception ex)
            {
                StartupLog.Log("Failed to ensure base directory", ex);
            }
        }
    }

    internal static class StartupLog
    {
        private static readonly string LogPath = Path.Combine(Config.BaseDir, "app.log");

        public static void Log(string message, Exception? ex = null)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                var text = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}";
                if (ex != null)
                {
                    text += $"{Environment.NewLine}{ex}";
                }
                File.AppendAllText(LogPath, text + Environment.NewLine + Environment.NewLine);
            }
            catch
            {
                // ignore logging failures
            }
        }
    }
}

