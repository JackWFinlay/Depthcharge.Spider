using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Depthcharge.Spider.Controllers
{

    [Route("api/[controller]")]
    public class SpiderController : Controller
    {
        
        [HttpGet]
        public void StartCrawl([FromServices] IOptions<DocumentDBSettings> appSettings)
        {
            DocumentDbClient documentDbClient = new DocumentDbClient(appSettings);
            SetupAsync().Wait();

            while (!Spider.StopCrawl)
            {
                try
                {
                    Task crawlTask = Task.Run(() => Crawl(appSettings, documentDbClient));
                    crawlTask.Wait();
                }
                catch (Exception)
                {
                    break;
                }
            } 
        }

        [HttpPost]
        public void StopCrawl()
        {
            Spider.StopCrawl = true;
        }

        private static async Task SetupAsync()
        {
            await DocumentDbClient.SetupAsync();
        }

        private static async Task Crawl(IOptions<DocumentDBSettings> appSettings, DocumentDbClient documentDbClient)
        {
            Spider spider = new Spider(appSettings);
            await spider.Run(documentDbClient);
        }
    }
}
