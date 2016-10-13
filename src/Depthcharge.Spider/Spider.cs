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

        public Spider(IOptions<DocumentDBSettings> appSettings)
        {
            _documentDbClient = new DocumentDbClient(appSettings);
        }

        public async Task Run(DocumentDbClient documentDbClient)
        {
            _documentDbClient = documentDbClient;
            string url = NextUrlToIndex();

            if (url == null)
            {
                return;
            }

            string html = await GetHtmlStringForUrlAsync(new Uri(url));
            JsonizeNode jsonizeNode = JsonizeHtml(html);
            IndexDocument indexDocument = new IndexDocument(jsonizeNode, url);

            Task indexTask = _documentDbClient.CreateIndexingDocumentAsync(DocumentDbClient.DbName, 
                                                                    DocumentDbClient.IndexDocumentCollectionName, 
                                                                    indexDocument);

            Task updateQueueTask = UpdateQueue(url);
        }


        private static JsonizeNode JsonizeHtml(string htmlString)
        {
            var jsonize = new Jsonize(htmlString);
            return jsonize.ParseHtmlAsJsonizeNode();
        }

        private static async Task<string> GetHtmlStringForUrlAsync(Uri url)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                return await response.Content.ReadAsStringAsync();
            }
        }

        private static string NextUrlToIndex()
        {
            //Document queue = DocumentClient.CreateDocumentQuery<Document>(IndexQueueCollectionLink)
            //                .OrderByDescending(x => x)
            //                .SingleOrDefault();

            //return queue?.GetPropertyValue<List<QueueItem>>("QueueItems").OrderByDescending(x => x.Priority).FirstOrDefault()?.Url;
            return @"http://jackfinlay.com";
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
