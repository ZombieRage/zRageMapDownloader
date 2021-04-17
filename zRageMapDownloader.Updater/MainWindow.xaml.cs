using GitHub.ReleaseDownloader;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace zRageMapDownloader.Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _registryPath = @"Software\ZombieRageBrasil\MapDownloader";
        private readonly string _versionKey = "Version";

        private readonly string _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private readonly string _appFolderName = "zRageMapDownloader";
        private readonly string _appFileName = "ZRAGE.BRASIL.Map.Downloader.exe";
        private string _appPath => Path.Combine(_appDataPath, _appFolderName);
        private string _appFile => Path.Combine(_appPath, _appFileName);

        public MainWindow()
        {
            Visibility = Visibility.Hidden;
            Directory.CreateDirectory(_appPath);

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
            bool includePreRelease = true;
            IReleaseDownloaderSettings settings = new ReleaseDownloaderSettings(httpClient, author, repo, includePreRelease, _appPath);

            // create downloader
            IReleaseDownloader downloader = new ReleaseDownloader(settings);

            // check version
            DispatchIfNecessary(() => 
            {
                txtMessage.Text = "Checking for updates..."; 
            });

            string currentVersion = GetVersionKeyInfo();
            bool installNewVersion = !string.IsNullOrEmpty(currentVersion) ? !downloader.IsLatestRelease(currentVersion) : true;
            if (installNewVersion || !File.Exists(_appFile))
            {
                DispatchIfNecessary(() => 
                {
                    Visibility = Visibility.Visible;
                    txtMessage.Text = "Downloading lastest version..."; 
                });

                downloader.DownloadLatestRelease();

                var downloadedVersion = downloader.GetLatestReleaseVersion();
                UpdateVersionKeyInfo(downloadedVersion);
            }

            if (File.Exists(_appFile))
            {
                DispatchIfNecessary(() =>
                {
                    txtMessage.Text = "Starting application...";
                    pbProgress.IsIndeterminate = false;
                    pbProgress.Value = 1;
                });

                ProcessStartInfo psi = new ProcessStartInfo(_appFile);
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
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(_registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
            reg.SetValue(_versionKey, version);
        }

        private string GetVersionKeyInfo()
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(_registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
            return reg.GetValue(_versionKey)?.ToString();
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
            var procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_appFileName));
            foreach (var proc in procs)
            {
                proc.Kill();
            }
        }
    }
}
