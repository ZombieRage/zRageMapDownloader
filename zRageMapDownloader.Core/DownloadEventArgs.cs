using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zRageMapDownloader.Core
{
    public class DownloadEventArgs : EventArgs
    {
        public string Filename { get; private set; }
        public int Progress { get; private set; }

        public DownloadEventArgs(int progress, string filename)
        {
            Progress = progress;
            Filename = filename;
        }
        public DownloadEventArgs(int progress) : this(progress, String.Empty)
        {
            Progress = progress;
        }
    }
}
