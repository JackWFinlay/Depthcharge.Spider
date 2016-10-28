using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Depthcharge.Spider
{
    public class DocumentDbSettings : IDocumentDbSettings
    {
        public string DocumentDbConnectionString { get; set; }
        public string DocumentDbPrimaryKey { get; set; }
        public string DbName { get; set; }
        public string CollectionName { get; set; }
    }
}
