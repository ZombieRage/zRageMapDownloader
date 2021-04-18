using System.ComponentModel;
using System.IO;

namespace zRageMapDownloader.Core
{
    public class MapModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ServerModel _serverContext;
        public string Name { get; private set; }
        public string DownloadableFileName { get; private set; }
        public string LocalFileName { get; private set; }
        public bool IsCompressed { get; private set; }
        public string RemoteFileName { get; private set; }
        public bool SkipOnDownload { get; set; } = false;

        // Display porpuses
        public bool Visible { get; set; } = true;

        public MapModel(string rawName, ServerModel serverContext)
        {
            _serverContext = serverContext;

            var fileExt = ".bsp";
            IsCompressed = rawName[0] != '$';

            if (IsCompressed)
            {
                fileExt += ".bz2";
            }

            // removes the $ prefix if exists
            Name = rawName.Replace("$", "");

            DownloadableFileName = Name + fileExt;
            LocalFileName = Name + ".bsp";
            RemoteFileName = _serverContext.FastdlUrl + DownloadableFileName;
        }

        public bool ExistsInMapsFolder(string mapsFolder)
        {
            var mapsPath = mapsFolder ?? _serverContext.GetMapsDirectory();
            var exists = File.Exists(Path.Combine(mapsPath, LocalFileName));

            return exists;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
