using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zRageMapDownloader.Core
{
    public class ServersJson
    {
        [JsonProperty("servers")]
        public List<ServerModel> Servers { get; set; }
    }
}
