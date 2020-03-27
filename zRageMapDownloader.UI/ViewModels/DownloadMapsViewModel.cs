using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using zRageMapDownloader.Commands;
using zRageMapDownloader.Core;

namespace zRageMapDownloader.ViewsModels
{
    public class DownloadMapsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MapManager _mm;
        public ServerModel Server { get; set; }
        public string MapsDirectory { get; set; }
        public bool ReplaceExistingMaps { get; set; }
        public string Log { get; set; }
        public int Progress { get; set; }
        public int MapsToDownload { get; set; }
        public bool DownloadInProgress { get; set; }
        public bool Cancelling { get; set; }

        public OpenFolderDialogCommand OpenFolderDialogCommand { get; set; }
        public StartDownloadCommand StartDownloadCommand { get; set; }
        public CancelDownloadCommand CancelDownloadCommand { get; set; }

        public DownloadMapsViewModel()
        {
            OpenFolderDialogCommand = new OpenFolderDialogCommand(this);
            StartDownloadCommand = new StartDownloadCommand(this);
            CancelDownloadCommand = new CancelDownloadCommand(this);

            Progress = 0;
            MapsToDownload = 1;
        }

        public void BindServerObject(ServerModel server)
        {
            Server = server;
            MapsDirectory = server.GetMapsDirectory();

            _mm = new MapManager(Server);
        }

        public void SelectMapsFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.ShowDialog();

                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    return;
                }

                MapsDirectory = dialog.SelectedPath;
            }
        }

        public void AppendToLog(string message)
        {
            Log += $"{message}{Environment.NewLine}";
        }

        public void CancelDownload()
        {
            AppendToLog("Cancelation pending. Waiting for end of the current process..." + Environment.NewLine);
            Cancelling = true;
            _mm.Cancel();
        }

        public async void StartDownload()
        {
            Progress = 0;
            DownloadInProgress = true;
            List<string> maps = null;

            try
            {
                maps = Server.GetMapsToDownload().Where(x => !string.IsNullOrEmpty(x) || x.Length > 3).ToList();
                MapsToDownload = maps.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error when trying to retrieve information from the file {Server.MapListUrl}: {ex.Message}");
                return;
            }

            if (!maps.Any())
            {
                MessageBox.Show($"The current map list found at {Server.MapListUrl} has no map");
                return;
            }

            foreach (var map in maps)
            {
                var normalizedName = map.Replace("$", "");

                if (_mm.Canceled)
                {
                    AppendToLog("Download cancelled by the user." + Environment.NewLine);
                    Progress = 0;
                    DownloadInProgress = false;
                    Cancelling = false;
                    return;
                }

                var existingFile = Path.Combine(Server.GetMapsDirectory(), normalizedName + ".bsp");
                if (!ReplaceExistingMaps && File.Exists(existingFile))
                {
                    AppendToLog($"{normalizedName} already exists. Skipping...");
                    Progress++;
                    continue;
                }

                try
                {
                    await Task.Run(() => 
                    {
                        AppendToLog($"Downloading {normalizedName}...");

                        _mm.Download(map).Wait();

                        Thread.Sleep(1000);

                        if (map[0] != '$')
                        {
                            AppendToLog($"Decompressing {normalizedName}...");
                            _mm.Decompress(normalizedName);
                        }

                        Thread.Sleep(1000);

                        AppendToLog($"Moving {normalizedName} to maps folder...");
                        if (!_mm.MoveToMapsFolder(normalizedName))
                        {
                            throw new Exception($"Can't move to maps folder");
                        }
                    });
                }
                catch (Exception ex)
                {
                    AppendToLog($"Error while processing {normalizedName}: {ex.Message}");
                    AppendToLog($"Skipping {normalizedName}");
                }

                Progress++;
            }

            AppendToLog($"Download finished.");
            DownloadInProgress = false;
        }
    }
}
