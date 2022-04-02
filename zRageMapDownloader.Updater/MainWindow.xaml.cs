using GitHub.ReleaseDownloader;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using zRageMapDownloader.Core;

namespace zRageMapDownloader.Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            RegisterCustomUrlScheme();
            Visibility = Visibility.Hidden;
            Directory.CreateDirectory(Utils.APP_PATH);


            CloseApplicationIfOpen();
            InitializeComponent();

            pbProgress.Minimum = 0;
            pbProgress.Maximum = 1;
            pbProgress.IsIndeterminate = true;

            Task.Run(() =>
            {
                CheckUpdatesAndContinues();
            });
        }

        private void CheckUpdatesAndContinues()
        {
            HttpClient httpClient = new HttpClient();
            string author = "ZombieRage";
            string repo = "zRageMapDownloader";
            bool includePreRelease = false;
            IReleaseDownloaderSettings settings = new ReleaseDownloaderSettings(httpClient, author, repo, includePreRelease, Utils.APP_PATH);

            // create downloader
            IReleaseDownloader downloader = new ReleaseDownloader(settings);

            // check version
            DispatchIfNecessary(() => 
            {
                txtMessage.Text = "Checking for updates..."; 
            });

            string currentVersion = GetVersionKeyInfo();
            bool installNewVersion = !string.IsNullOrEmpty(currentVersion) ? !downloader.IsLatestRelease(currentVersion) : true;
            if (installNewVersion || !File.Exists(Utils.APP_FILE))
            {
                DispatchIfNecessary(() => 
                {
                    Visibility = Visibility.Visible;
                    txtMessage.Text = "Downloading lastest version..."; 
                });

                downloader.DownloadLatestRelease();

                var downloadedVersion = downloader.GetLatestReleaseVersion();
                UpdateVersionKeyInfo(downloadedVersion);
                RegisterCustomUrlScheme();
            }

            if (File.Exists(Utils.APP_FILE))
            {
                DispatchIfNecessary(() =>
                {
                    txtMessage.Text = "Starting application...";
                    pbProgress.IsIndeterminate = false;
                    pbProgress.Value = 1;
                });

                ProcessStartInfo psi = new ProcessStartInfo(Utils.APP_FILE);
                Process.Start(psi);
            }
            else
            {
                DispatchIfNecessary(() => 
                { 
                    MessageBox.Show("Application not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error); 
                });
            }

            // clean up
            downloader.DeInit();
            httpClient.Dispose();
            DispatchIfNecessary(() =>
            {
                Application.Current.Shutdown();
            });
        }

        private void UpdateVersionKeyInfo(string version)
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(Utils.REGISTRY_PATH, RegistryKeyPermissionCheck.ReadWriteSubTree);
            reg.SetValue(Utils.VERSION_KEY, version);
        }

        private string GetVersionKeyInfo()
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(Utils.REGISTRY_PATH, RegistryKeyPermissionCheck.ReadWriteSubTree);
            return reg.GetValue(Utils.VERSION_KEY)?.ToString();
        }

        private void DispatchIfNecessary(Action action)
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(action);
            else
                action.Invoke();
        }

        private void CloseApplicationIfOpen()
        {
            var procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Utils.APP_FILE_NAME));
            foreach (var proc in procs)
            {
                proc.Kill();
            }
        }

        private void RegisterCustomUrlScheme()
        {
            try
            {
                var mainKey = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
                RegistryKey key = mainKey.CreateSubKey(Utils.CUSTOM_URL_KEY);
                key.SetValue("URL Protocol", "");
                key.CreateSubKey(@"shell\open\command").SetValue("", $"{Utils.APP_FILE} %1");
            }
            catch (System.Exception e)
            {
                MessageBox.Show($"Error while registering custom URL: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
