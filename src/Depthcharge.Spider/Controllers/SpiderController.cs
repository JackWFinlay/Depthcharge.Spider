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

        private static IOptions<ServiceSettings> _serviceSettings;
        private static IOptions<DocumentDBSettings> _dbSettings;
        private static int _threadCount = 0;

        public SpiderController([FromServices] IOptions<DocumentDBSettings> dbSettings, [FromServices] IOptions<ServiceSettings> serviceSettings)
        {
            if (_serviceSettings == null)
            {
                _serviceSettings = serviceSettings;
            }

            if (_dbSettings == null)
            {
                _dbSettings = dbSettings;
            }
        }

        [HttpGet]
        public string Crawl( )
        {
            StartCrawl();
            return "Spider started.";
        }

        [HttpGet]
        public string Halt()
        {
            Spider.StopCrawl = true;
            return "Spider halted.";
        }

        private static void StartCrawl()
        {
            //Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async i =>
            //{
            //    await DoCrawl();
            //});

            ParallelWhile(new ParallelOptions { MaxDegreeOfParallelism = 4 }, GetStopCrawlStatus);
        }

        private static async Task DoCrawl()
        {

            try
            {
                Spider spider = new Spider(_dbSettings, _serviceSettings);
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


        public static void ParallelWhile(ParallelOptions parallelOptions, Func<bool> condition)
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
