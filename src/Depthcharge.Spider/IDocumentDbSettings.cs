using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Depthcharge.Spider
{
    public interface IDocumentDbSettings
    {
        string DocumentDbConnectionString { get; set; }
        string DocumentDbPrimaryKey { get; set; }
        string DbName { get; set; }
        string CollectionName { get; set; }
    }
}
