using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Depthcharge.Spider
{
    public class QueueItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "protocol")]
        public string Protocol { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "priority")]
        public int Priority { get; set; }

        [JsonProperty(PropertyName = "indexed")]
        public bool Indexed { get; set; }

        public QueueItem(string url)
        {
            string[] split;
            if (url.StartsWith("http://"))
            {
                split = url.Split(new string[] { "http://" }, StringSplitOptions.None);
                Protocol = "http";
                Url = split[1];
            }
            else if (url.StartsWith("https://"))
            {
                split = url.Split(new string[] {"https://"}, StringSplitOptions.None);
                Protocol = "https";
                Url = split[1];
            }
            else
            {
                Url = url;
                Protocol = "http";
            }

            Priority = 1;
            Indexed = false;
        }
    }
}
