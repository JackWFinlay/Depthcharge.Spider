using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JackWFinlay.Jsonize;

namespace Depthcharge.Spider
{
    public class IndexDocument : JackWFinlay.Jsonize.JsonizeMeta
    {
        public DateTime DateTime { get; set; }

        public IndexDocument(JsonizeNode jsonizeNode, string url) : base(jsonizeNode, url)
        {
            DateTime = DateTime.Now;
        }
    }
}
