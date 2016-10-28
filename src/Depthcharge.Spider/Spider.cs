using JackWFinlay.Jsonize;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Depthcharge.Spider
{
    public class Spider
    {
        private static IDocumentDbClient _documentDbClient;
        private static IServiceSettings _serviceSettings;
        private static IDocumentDbSettings _documentDbSettings;
        public static bool StopCrawl = false;

        public Spider(IDocumentDbClient documentDbClient, IDocumentDbSettings documentDbSettings, IServiceSettings serviceSettings )
        {
            _documentDbClient = documentDbClient;
            _documentDbSettings = documentDbSettings;
            _serviceSettings = serviceSettings;
        }

        public async Task Run()
        {
            QueueItem queueItem = await NextUrlToIndex();

            if (queueItem == null)
            {
                Console.WriteLine("No items in queue.");
                return;
            }

            string url = $"{queueItem.Protocol}://{queueItem.Url}";
            string html = await GetContentForUrlAsync(new Uri(url));

            if (html == null)
            {
                return;
            }

            JsonizeNode jsonizeNode = JsonizeHtml(html);
            IndexDocument indexDocument = new IndexDocument(jsonizeNode, url);

            Task indexTask = _documentDbClient.CreateIndexingDocumentAsync(_documentDbSettings.DbName, 
                                                                    _documentDbSettings.CollectionName, 
                                                                    indexDocument);

            List<QueueItem> linksList = GetQueueItemsFromIndexDocument(indexDocument);

            UpdateQueue(queueItem).Wait();
            Task postToQueueTask = PostToQueue(linksList);
            

            Task.WaitAll(indexTask, postToQueueTask);

        }

        private static async Task PostToQueue(List<QueueItem> linksList)
        {
            
                string json = "";
                if (linksList != null)
                {
                    json = JsonConvert.SerializeObject(linksList);
                }

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var method = new HttpMethod("POST");
                var request = new HttpRequestMessage(method, _serviceSettings.QueueManagerUrl)
                {
                    Content = content
                };

                using (HttpClient client = new HttpClient())
                {
                    await client.SendAsync(request);
                }
        }

        private static List<QueueItem> GetQueueItemsFromIndexDocument(IndexDocument indexDocument)
        {
            List<QueueItem> linksList = new List<QueueItem>();
            GetQueueItemsFromJsonizeNode(indexDocument.DocumentJsonizeNode, linksList);
            return linksList;
        }

        private static void GetQueueItemsFromJsonizeNode(JsonizeNode parentNode, List<QueueItem> linksList)
        {
            if (parentNode.Children != null)
            {
                foreach (JsonizeNode childNode in parentNode.Children)
                {
                    if (childNode.Tag != null && childNode.Tag.Equals("a"))
                    {
                        IDictionary<string, object> attributesDictionary = childNode.Attributes;
                        if (!attributesDictionary["href"].ToString().ToLower().Contains("mailto:") &&
                            attributesDictionary["href"].ToString().ToLower().StartsWith("http"))
                        {
                            linksList.Add(new QueueItem(attributesDictionary["href"].ToString()));
                        }
                    }

                    GetQueueItemsFromJsonizeNode(childNode, linksList);
                }
            }
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
                
            }
            return await response.Content.ReadAsStringAsync();

        }

        private static async Task<QueueItem> NextUrlToIndex()
        {
            string json = await GetContentForUrlAsync(new Uri(_serviceSettings.QueueManagerUrl));

            return json != null ? JsonConvert.DeserializeObject<QueueItem>(json) : null;
        }

        private static async Task UpdateQueue(QueueItem queueItem)
        {
            string json = "";
            if (queueItem != null)
            {
                json = JsonConvert.SerializeObject(queueItem);
            }

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, _serviceSettings.QueueManagerUrl)
            {
                Content = content
            };

            using (var client = new HttpClient())
            {
                await client.SendAsync(request);
            }
        }
    }
}
