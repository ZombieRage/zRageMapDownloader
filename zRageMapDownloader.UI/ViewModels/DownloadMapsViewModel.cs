﻿using System;
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
            DownloadInProgress = true;
            List<string> maps = null;

            try
            {
                maps = Server.GetMapsToDownload();
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
                if (_mm.Canceled)
                {
                    AppendToLog("Download cancelled by the user." + Environment.NewLine);
                    Progress = 0;
                    DownloadInProgress = false;
                    Cancelling = false;
                    return;
                }

                var existingFile = Path.Combine(Server.GetMapsDirectory(), map + ".bsp");
                if (!ReplaceExistingMaps && File.Exists(existingFile))
                {
                    AppendToLog($"{map} already exists. Skipping...");
                    Progress++;
                    continue;
                }

                try
                {
                    await Task.Run(() => 
                    {
                        AppendToLog($"Downloading {map}...");

                        _mm.Download(map).Wait();

                        Thread.Sleep(1000);
                        AppendToLog($"Decompressing {map}...");
                        _mm.Decompress(map);

                        AppendToLog($"Moving {map} to maps folder...");
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

            AppendToLog($"Download finished.");
            DownloadInProgress = false;
        }
    }
}
