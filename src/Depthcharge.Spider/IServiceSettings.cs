using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Depthcharge.Spider
{
    public interface IServiceSettings
    {
        string QueueManagerUrl { get; set; }
    }
}
