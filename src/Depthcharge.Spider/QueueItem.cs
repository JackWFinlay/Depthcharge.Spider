using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Depthcharge.Spider
{
    public class QueueItem
    {
        public string Url { get; set; }
        public int Priority { get; set; }
    }
}
