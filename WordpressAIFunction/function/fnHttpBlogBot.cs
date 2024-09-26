using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Common.Content.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace WordpressAIFunction
{
    /// <summary>
    /// Azure Function class responsible for handling HTTP requests to trigger the blog bot.
    /// </summary>
    public class fnBlogBot
    {
        private BlogManager _blogManager;
        private ILogger<fnBlogBot> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="fnBlogBot"/> class.
        /// </summary>
        /// <param name="blogManager">The blog manager instance.</param>
        /// <param name="logger">The logger instance.</param>
        public fnBlogBot(BlogManager blogManager, ILogger<fnBlogBot> logger)
        {
            _blogManager = blogManager;
            _logger = logger;
        }

        /// <summary>
        /// HTTP trigger function to run the blog bot.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [FunctionName("fnBlogBot")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            try
            {
                await _blogManager.RunAsync();
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Successfully Posted!")
                };
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                return new ObjectResult("Failed") { StatusCode = 501 };
            }
        }
    }
}

