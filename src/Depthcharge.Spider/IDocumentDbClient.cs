using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Depthcharge.Spider
{
    public interface IDocumentDbClient
    {
        //private static readonly string EndpointUri = Configuration.GetValue<string>("documentDBConnectionString") ?? Environment.GetEnvironmentVariable("APPSETTING_documentDBconnectionString");

        //private static readonly string PrimaryKey =
        // ConfigurationManager.AppSettings["documentDBPrimaryKey"] ?? Environment.GetEnvironmentVariable("APPSETTING_documentDBPrimaryKey");

        Task CreateIndexingDocumentAsync(string databaseName, 
                                        string collectionName,
                                        IndexDocument indexDocument);
    }
}
