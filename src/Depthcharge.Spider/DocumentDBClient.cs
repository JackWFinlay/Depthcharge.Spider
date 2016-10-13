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
    public class DocumentDbClient
    {
        //private static readonly string EndpointUri = Configuration.GetValue<string>("documentDBConnectionString") ?? Environment.GetEnvironmentVariable("APPSETTING_documentDBconnectionString");

        //private static readonly string PrimaryKey =
        // ConfigurationManager.AppSettings["documentDBPrimaryKey"] ?? Environment.GetEnvironmentVariable("APPSETTING_documentDBPrimaryKey");

        internal static string DbName = "Depthcharge";
        internal static DocumentClient DocumentClient;
        internal static Uri IndexQueueCollectionLink;
        internal static Uri IndexDocumentcollectionLink;
        internal static string QueueCollectionName = "IndexQueue";
        internal static string IndexDocumentCollectionName = "IndexDocuments";

        public DocumentDbClient(IOptions<DocumentDBSettings> appSettings)
        {
            DocumentDBSettings documentDbSettings = appSettings.Value;
            DocumentClient = new DocumentClient(new Uri(documentDbSettings.DocumentDBConnectionString), documentDbSettings.DocumentDBPrimaryKey);
        }

       

        public static async Task SetupAsync()
        {
            await CreateDatabaseIfNotExistsAsync(DbName);
            //await CreateDocumentCollectionIfNotExistsAsync(DbName, QueueCollectionName);
            await CreateDocumentCollectionIfNotExistsAsync(DbName, IndexDocumentCollectionName);
            //IndexQueueCollectionLink = UriFactory.CreateDocumentCollectionUri(DbName, QueueCollectionName);
            IndexDocumentcollectionLink = UriFactory.CreateDocumentCollectionUri(DbName, IndexDocumentCollectionName);
        }

        private static async Task CreateDatabaseIfNotExistsAsync(string databaseName)
        {
            // Check to verify a database with the id=FamilyDB does not exist
            try
            {
                await DocumentClient.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
            }
            catch (DocumentClientException de)
            {
                // If the database does not exist, create a new database
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await DocumentClient.CreateDatabaseAsync(new Database { Id = databaseName });
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task CreateDocumentCollectionIfNotExistsAsync(string databaseName, string collectionName)
        {
            try
            {
                await DocumentClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName));
            }
            catch (DocumentClientException de)
            {
                // If the document collection does not exist, create a new collection
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    DocumentCollection collectionInfo = new DocumentCollection
                    {
                        Id = collectionName,
                        IndexingPolicy = new IndexingPolicy(new RangeIndex(Microsoft.Azure.Documents.DataType.String) {Precision = -1})
                    };

                    // Configure collections for maximum query flexibility including string range queries.

                    // Here we create a collection with 400 RU/s.
                    await DocumentClient.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseName),
                        collectionInfo,
                        new RequestOptions { OfferThroughput = 400 });
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task CreateQueueDocumentIfNotExistsAsync(string databaseName, string collectionName, Queue queue )
        {
            try
            {
                await DocumentClient.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, queue.Id));
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await DocumentClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queue);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task CreateIndexingDocumentAsync(string databaseName, string collectionName, IndexDocument indexDocument)
        { 
            await DocumentClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), indexDocument);  
        }
    }
}
