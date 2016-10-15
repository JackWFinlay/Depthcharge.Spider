using JackWFinlay.Jsonize;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Depthcharge.Spider
{
    public class Spider
    {
        private DocumentDbClient _documentDbClient;
        public static bool StopCrawl = false;


        public Spider(IOptions<DocumentDBSettings> dbSettings, IOptions<ServiceSettings> serviceSettings)
        {
            _documentDbClient = new DocumentDbClient(dbSettings);

        }

        public async Task Run(DocumentDbClient documentDbClient)
        {
            _documentDbClient = documentDbClient;
            string url = await NextUrlToIndex();

            if (url == null)
            {
                return;
            }

            string html = await GetContentForUrlAsync(new Uri(url));

            if (html == null)
            {
                return;
            }

            JsonizeNode jsonizeNode = JsonizeHtml(html);
            IndexDocument indexDocument = new IndexDocument(jsonizeNode, url);

            Task indexTask = _documentDbClient.CreateIndexingDocumentAsync(DocumentDbClient.DbName, 
                                                                    DocumentDbClient.IndexDocumentCollectionName, 
                                                                    indexDocument);

            Task updateQueueTask = UpdateQueue(url);

            Task.WaitAll(indexTask, updateQueueTask);
        }


        private static JsonizeNode JsonizeHtml(string htmlString)
        {
            var jsonize = new Jsonize(htmlString);
            return jsonize.ParseHtmlAsJsonizeNode();
        }

        private static async Task<string> GetContentForUrlAsync(Uri url)
        {
            HttpResponseMessage response;
            try
            {
                using (var client = new HttpClient())
                {
                    response = await client.GetAsync(url);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return await response.Content.ReadAsStringAsync();

        }

        private static async Task<string> NextUrlToIndex()
        {
            
            return await GetContentForUrlAsync(new Uri("http://localhost:5001/Queue"));
        }

        private async Task UpdateQueue(string url)
        {
            //Document doc = DocumentClient.CreateDocumentQuery<Document>(IndexQueueCollectionLink)
            //                .AsEnumerable()
            //                .SingleOrDefault();

            ////Update some properties on the found resource
            //doc?.SetPropertyValue(url, (doc.GetPropertyValue<int>("Priority") + 1));

            //if (doc?.GetPropertyValue<string>("Url") == null)
            //{
            //    doc.
            //}


            ////Now persist these changes to the database by replacing the original resource
            //await DocumentClient.ReplaceDocumentAsync(doc);
            Console.WriteLine("Updating Queue");
        }
    }
}
