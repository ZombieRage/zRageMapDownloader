using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace zRageMapDownloader.Core
{
    public static class ServerModelExtensions
    {
        public static List<string> GetMapsToDownload(this ServerModel server)
        {
            var client = new WebClient();
            var csvMaps = client.DownloadString(server.MapListUrl);
            client.Dispose();

            var list = csvMaps
                .Replace("\r", "")
                .Replace("\n", "")
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x) || x.Length > 3)
                .ToList();

            return list;
        }

        public static string GetMapsDirectory(this ServerModel serverModel)
        {
            var lastMapsFolder = MapManager.GetLastMapsFolder();
            if (lastMapsFolder == null || !Directory.Exists(lastMapsFolder))
            {
                return "";
            }

            return lastMapsFolder;
        }
    }
}
