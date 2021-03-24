using CDT.Cosmos.Cms.Common.Controllers;
using CDT.Cosmos.Cms.Common.Data.Logic;
using CDT.Cosmos.Cms.Common.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CDT.Cosmos.Cms.Website.Controllers
{
    public class HomeController : CosmosController
    {
        public HomeController(ILogger<HomeController> logger,
            ArticleLogic articleLogic,
            IOptions<RedisContextConfig> redisOptions,
            IOptions<GoogleCloudAuthConfig> gglOptions,
            IOptions<SimpleProxyConfigs> proxyConfigs) :
            base(logger, articleLogic: articleLogic, redisOptions, gglOptions, proxyConfigs)
        {
        }

        //
        // Comment this out as this is handled by the base class.
        //
        //public IActionResult Index()
        //{
        //    return View();
        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("Error/{statusCode}")]
        public IActionResult HandleErrorCode(int statusCode)
        {
            var statusCodeData = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Sorry the page you requested could not be found";
                    ViewBag.RouteOfException = statusCodeData.OriginalPath;
                    break;
                case 500:
                    ViewBag.ErrorMessage = "Sorry something went wrong on the server";
                    ViewBag.RouteOfException = statusCodeData.OriginalPath;
                    break;
            }

            return View("Error");
        }
    }
}