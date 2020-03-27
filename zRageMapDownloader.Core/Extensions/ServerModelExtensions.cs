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

            var list = csvMaps.Replace("\r", "").Replace("\n", "").Split(',').ToList();

            return list;
        }

        public static string BuildMapFile(this ServerModel server, string mapName)
        {
            bool isCompressed = mapName[0] != '$';

            if (!isCompressed)
            {
                mapName = mapName.Remove(0, 1);
            }

            return $"{mapName}.bsp{(isCompressed ? ".bz2" : "")}";
        }

        public static string GetMapsDirectory(this ServerModel server)
        {
            string registryValue = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null);

            if (registryValue == null)
            {
                return null;
            }

            string libraryInfoFileContents = File.ReadAllText(registryValue.Replace("/", @"\") + @"\steamapps\libraryfolders.vdf");
            List<string> libraryFolders = new List<string>();

            for (int i = 1; true; i++)
            {
                if (!libraryInfoFileContents.Contains("\"" + i + "\""))
                {
                    break;
                }

                libraryFolders.Add(libraryInfoFileContents.Split(new string[] { "\"" + i + "\"		\"" }, StringSplitOptions.None)[1].Split('"')[0].Replace(@"\\", @"\"));
            }

            libraryFolders.Add(registryValue.Replace("/", @"\"));

            string dir = null;
            foreach (string folder in libraryFolders)
            {
                string acfFile = folder + $@"\steamapps\appmanifest_{server.SteamApplicationID}.acf";

                if (File.Exists(acfFile))
                {
                    string installDir = File.ReadAllText(acfFile).Split(new string[] { "\"installdir\"		\"" }, StringSplitOptions.None)[1].Split('"')[0];

                    dir = folder.Substring(0, 1).ToUpper() + folder.Substring(1).Replace("program files", "Program Files") + $@"\steamapps\common\{installDir}{server.MapsDirectory}";
                    break;
                }
            }

            return dir;
        }
    }
}
