using System;
using System.IO;
using System.Linq;

namespace zRageMapDownloader.Core
{
    public static class Utils
    {
        public static readonly string REGISTRY_PATH = @"Software\ZombieRageBrasil\MapDownloader";
        public static readonly string VERSION_KEY = "Version";
        public static readonly string MAPS_FOLDER_KEY = "MapsFolder";
        public static readonly string CUSTOM_URL_KEY = "zragedl";

        public static readonly string APP_DATA_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static readonly string APP_FOLDER_NAME = "zRageMapDownloader";
        public static readonly string APP_FILE_NAME = "ZRAGE.BRASIL.Map.Downloader.exe";
        public static string APP_PATH => Path.Combine(APP_DATA_PATH, APP_FOLDER_NAME);
        public static string APP_FILE => Path.Combine(APP_PATH, APP_FILE_NAME);

        private static readonly string _defaultRegistryKey = "ServersContextRemoteFile";
        public static readonly string DefaultServersContextRemoteFile = "https://raw.githubusercontent.com/quasemago/zrageservers/master/MapDownloaderApp/servers.json";
        public static string GetServersContextRemoteFile()
        {
            return DefaultServersContextRemoteFile;
            /*RegistryKey reg = Registry.CurrentUser.CreateSubKey(@"Software\ZombieRageBrazil\MapDownloader", RegistryKeyPermissionCheck.ReadWriteSubTree);

            // get and save the default server context remote file in case of non existent ServersContextRemoteFile registry 
            var remoteFile = reg.GetValue(_defaultRegistryKey);
            if (remoteFile == null)
            {
                reg.SetValue(_defaultRegistryKey, DefaultServersContextRemoteFile, RegistryValueKind.String);
                remoteFile = reg.GetValue(_defaultRegistryKey);

                if (remoteFile == null)
                {
                    // in case something goes wrong
                    remoteFile = DefaultServersContextRemoteFile;
                }
            }

            return remoteFile as string; */
        }

        public static string NormalizeUrl(string url)
        {
            if (url.Last() != '/')
            {
                url += '/';
            }

            return url;
        }
    }
}
