using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using zRageMapDownloader.Commands;
using zRageMapDownloader.Core;
using zRageMapDownloader.ViewModels;
using zRageMapDownloader.Views;

namespace zRageMapDownloader.ViewModels
{
    public class DownloadMapsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MapManager _mm;
        public ServerModel Server { get; set; }
        public ObservableCollection<MapModel> Maps { get; set; }
        public string MapsDirectory { get; set; }
        public bool ReplaceExistingMaps { get; set; }
        public string Log { get; set; }
        public int Progress { get; set; }
        public int MapsToDownload { get; set; }
        public bool DownloadInProgress { get; set; }
        public bool Cancelling { get; set; }

        public string MapsSelectionStatus { get; set; }

        public OpenFolderDialogCommand OpenFolderDialogCommand { get; set; }
        public StartDownloadCommand StartDownloadCommand { get; set; }
        public CancelDownloadCommand CancelDownloadCommand { get; set; }
        public OpenMapsSelectorCommand OpenMapsSelectorCommand { get; set; }

        public DownloadMapsViewModel()
        {
            OpenFolderDialogCommand = new OpenFolderDialogCommand(this);
            StartDownloadCommand = new StartDownloadCommand(this);
            CancelDownloadCommand = new CancelDownloadCommand(this);
            OpenMapsSelectorCommand = new OpenMapsSelectorCommand(this);
            Maps = new ObservableCollection<MapModel>();

            Progress = 0;
            MapsToDownload = 1;
        }

        public void BindServerObject(ServerModel server)
        {
            Server = server;
            MapsDirectory = server.GetMapsDirectory();

            _mm = new MapManager(Server);

            var listMaps = Server.GetMapsToDownload()
                    .Select(x => new MapModel(x, Server))
                    .ToList();

            Maps.Clear();
            foreach (var map in listMaps)
            {
                Maps.Add(map);
            }

            MapsSelectionStatus = $"{Maps.Count(x => !x.SkipOnDownload)} / {Maps.Count()} maps";
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
            AppendToLog("Cancelation pending. Waiting for end of the current process...");
            Cancelling = true;
            _mm.Cancel();
        }

        public void FinishDownloadAction()
        {
            Progress = 0;
            DownloadInProgress = false;
            AppendToLog($"Download finished.");
            AppendToLog(Environment.NewLine);
            _ = Task.Run(() => MapManager.DeleteAllTempFiles());
        }

        public async void StartDownload()
        {
            DownloadInProgress = true;
            _mm.Canceled = false;

            // filters the selected maps
            var maps = Maps.Where(x => !x.SkipOnDownload);
            MapsToDownload = maps.Count();

            if (!maps.Any())
            {
                AppendToLog("No map was selected for download.");
                return;
            }

            foreach (var map in maps)
            {
                if (_mm.Canceled)
                {
                    AppendToLog("Download cancelled by the user.");
                    Cancelling = false;
                    FinishDownloadAction();
                    return;
                }

                if (!ReplaceExistingMaps && map.ExistsInMapsFolder())
                {
                    AppendToLog($"{map} already exists. Skipping...");
                    Progress++;
                    continue;
                }

                try
                {
                    await Task.Run(() => 
                    {
                        AppendToLog($"Downloading {map.DownloadableFileName}...");

                        _mm.Download(map);

                        if (map.IsCompressed)
                        {
                            AppendToLog($"Decompressing {map.DownloadableFileName}...");
                            _mm.Decompress(map);
                        }

                        AppendToLog($"Moving {map.LocalFileName} to maps folder...");
                        if (!_mm.MoveToMapsFolder(map))
                        {
                            throw new Exception($"Can't move to maps folder");
                        }
                    });
                }
                catch (Exception ex)
                {
                    AppendToLog($"Error while processing {map}: {ex.Message}");
                    AppendToLog($"Skipping {map}");
                }

                Progress++;
            }

            FinishDownloadAction();
        }

        public void OpenMapsSelector()
        {
            var win = new WinMapsSelectorView();
            var vmMapsSelector = win.FindResource(nameof(MapsSelectorViewModel)) as MapsSelectorViewModel;
            vmMapsSelector.BindMapsObject(Maps);

            win.ShowDialog();
            MapsSelectionStatus = $"{Maps.Count(x => !x.SkipOnDownload)} / {Maps.Count()} maps";
        }
    }
}
