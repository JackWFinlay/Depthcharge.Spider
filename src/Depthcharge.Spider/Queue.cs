using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Depthcharge.Spider
{
    public class Queue
    {
        public string Id { get; set; }
        public List<QueueItem> QueueItems { get; set; }
    }
}
