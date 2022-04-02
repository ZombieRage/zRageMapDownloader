using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using zRageMapDownloader.Core;
using zRageMapDownloader.ViewModels;
using zRageMapDownloader.Views;

namespace zRageMapDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (Directory.Exists(MapManager.MainTempFolder))
            {
                Directory.Delete(MapManager.MainTempFolder, true);
            }

            Startup += Application_Startup;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Any())
            {
                if (e.Args.Count() == 1)
                {
                    var arg = e.Args[0];
                    if (arg.Contains(Core.Utils.CUSTOM_URL_KEY))
                    {
                        arg = arg.Replace($"{Core.Utils.CUSTOM_URL_KEY}:", "");
                    }

                    DownloadSingleMap(arg);
                }
            }
            else
            {
                var mainWin = new WinServerSelectionView();
                mainWin.Show();
            }
        }

        private void DownloadSingleMap(string mapStr)
        {
            var ssVm = new ServerSelectionViewModel();
            if (!ssVm.AvaliableServers.Any())
            {
                MessageBox.Show("There are no servers avaliable", "Server not found", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }

            // find server
            DownloadMapsViewModel dmVmSelected = null;
            IEnumerable<MapModel> mapsSelected = null;
            foreach (var server in ssVm.AvaliableServers)
            {
                var dmVm = new DownloadMapsViewModel();
                dmVm.BindServerObject(server);

                var mapsFound = dmVm.Maps?.Where(x => x.Name.Contains(mapStr));
                if (mapsFound.Any())
                {
                    mapsSelected = mapsFound;
                    dmVmSelected = dmVm;
                    break;
                }
            }

            if (dmVmSelected == null)
            {
                MessageBox.Show("Map not found in servers", "Map not found", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }

            // select map
            foreach (var map in dmVmSelected.Maps)
            {
                if (mapsSelected.Contains(map))
                {
                    map.SkipOnDownload = false;
                    continue;
                }

                map.SkipOnDownload = true;
            }

            // show progress
            var win = new WinDownloadMapsView();
            var vmDownload = win.FindResource(nameof(DownloadMapsViewModel)) as DownloadMapsViewModel;
            vmDownload.ForceRebind(dmVmSelected);
            vmDownload.AppendToLog("Automated map download...");
            win.Show();
            vmDownload.StartDownload();
            Thread.Sleep(300);
            win.Close();
        }
    }
}