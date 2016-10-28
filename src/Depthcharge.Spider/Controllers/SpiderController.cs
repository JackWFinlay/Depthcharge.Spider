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
        private readonly IDocumentDbClient _documentDbClient;
        private readonly IDocumentDbSettings _dbSettings;
        private readonly IServiceSettings _serviceSettings;

        private const int ThreadCount = 8;
        private static bool _spiderStarted = false;

        public SpiderController(IDocumentDbClient documentDbClient, IDocumentDbSettings dbSettings, IServiceSettings serviceSettings)
        {
            _documentDbClient = documentDbClient;
            _dbSettings = dbSettings;
            _serviceSettings = serviceSettings;
        }

        [HttpGet]
        public string Crawl()
        {
            string message = "";
            if (!_spiderStarted)
            {
                StartCrawl();
                _spiderStarted = true;
                message = "Spider started.";
            }
            else
            {
                message = "Spider already started.";
            }

            return message;
        }

        [HttpGet]
        public string Halt()
        {
            Spider.StopCrawl = true;
            _spiderStarted = false;
            return "Spider halted.";
        }

        private void StartCrawl()
        {
            //Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async i =>
            //{
            //    await DoCrawl();
            //});
            Spider.StopCrawl = false;
            ParallelWhile(new ParallelOptions { MaxDegreeOfParallelism = ThreadCount }, GetStopCrawlStatus);
            //ParallelWhile(new ParallelOptions(), GetStopCrawlStatus);
        }

        private async Task DoCrawl()
        {

            try
            {
                Spider spider = new Spider(_documentDbClient, _dbSettings, _serviceSettings);
                await spider.Run();
            }
            catch (Exception e)
            {
               Console.WriteLine(e.Message);
            }
        }

        private static bool GetStopCrawlStatus()
        {
            return !Spider.StopCrawl;
        }


        private void ParallelWhile(ParallelOptions parallelOptions, Func<bool> condition)
        {
            Parallel.ForEach(IterateUntilFalse(condition), parallelOptions,
                 ignored => DoCrawl().Wait());
        }

        private static IEnumerable<bool> IterateUntilFalse(Func<bool> condition)
        {
            while (condition()) yield return true;
        }
    }
}
