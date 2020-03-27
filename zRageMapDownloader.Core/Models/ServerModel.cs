using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zRageMapDownloader.Core
{
    public class ServerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mapList")]
        public string MapListUrl { get; set; }
        [JsonProperty("fastDL")]
        public string FastdlUrl { get; set; }
        [JsonProperty("appID")]
        public int SteamApplicationID { get; set; }
        [JsonProperty("mapsDirectory")]
        public string MapsDirectory { get; set; }
    }
}
