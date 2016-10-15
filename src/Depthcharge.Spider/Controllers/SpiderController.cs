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

    [Route("api/[controller]/[action]")]
    public class SpiderController : Controller
    {
        
        [HttpGet]
        public string Crawl([FromServices] IOptions<DocumentDBSettings> dbSettings, [FromServices] IOptions<ServiceSettings> serviceSettings )
        {
            StartCrawl(dbSettings, serviceSettings);

            return "Spider started.";
        }

        [HttpGet]
        public string Halt()
        {
            Spider.StopCrawl = true;
            return "Spider halted.";
        }

        private async void StartCrawl(IOptions<DocumentDBSettings> appSettings, IOptions<ServiceSettings> serviceSettings)
        {
            DocumentDbClient documentDbClient = new DocumentDbClient(appSettings);
            await SetupAsync();

            while (!Spider.StopCrawl)
            {
                try
                {
                    Task crawlTask = Task.Run(() => Crawl(appSettings, serviceSettings, documentDbClient));
                    crawlTask.Wait();
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private static async Task SetupAsync()
        {
            await DocumentDbClient.SetupAsync();
        }

        private static async Task Crawl(IOptions<DocumentDBSettings> dbSettings, IOptions<ServiceSettings> serviceSettings, DocumentDbClient documentDbClient)
        {
            Spider spider = new Spider(dbSettings, serviceSettings);
            await spider.Run(documentDbClient);
        }
    }
}
