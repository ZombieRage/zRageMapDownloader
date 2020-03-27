using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zRageMapDownloader.Core
{
    public static class Utils
    {
        private static readonly string _defaultRegistryKey = "ServersContextRemoteFile";
        public static readonly string DefaultServersContextRemoteFile = "http://dbi.zrage.com.br/servers.json";
        public static string GetServersContextRemoteFile()
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(@"Software\ZombieRageBrazil\MapDownloader", RegistryKeyPermissionCheck.ReadWriteSubTree);

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

            return remoteFile as string;
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
