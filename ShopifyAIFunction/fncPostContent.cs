using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Common.ShopifyAPI.Interface;
using Common.Content.Interface;
using Microsoft.Extensions.Configuration;

namespace ShopifyAIFunction
{
    public class fncShopifyContentRewriter
    {
        private readonly ILogger<fncShopifyContentRewriter> _logger;
        private readonly ShopifyContentProcessHandler _contentProcessor;
        private readonly IProcessHandler<int, string> _contentHandler;

        public fncShopifyContentRewriter(ILogger<fncShopifyContentRewriter> logger, IProcessHandler<int, string> contentHandler)
        {
            _logger = logger;
            _contentHandler = contentHandler;
        }

        [FunctionName("ShopifyPoster")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");

                if (!int.TryParse(req.Query["Days"], out int goBackDays) || goBackDays <= 0)
                {
                    goBackDays = 1;
                }

                var responseMessage = await _contentProcessor.ProcessRequest(goBackDays);
                return new OkObjectResult(responseMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                return new ObjectResult(ex.Message) { StatusCode = 500 };
            }
        }
    }
}
