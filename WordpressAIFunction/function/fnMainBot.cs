using System;
using System.Threading.Tasks;
using Common.Content.Interface;
using Microsoft.Azure.WebJobs;

namespace WordpressAIFunction.function
{
    /// <summary>
    /// Azure Function class responsible for running the main bot on a timer schedule.
    /// </summary>
    public class fnMainBot
    {
        private readonly IContentGenerator _openAPIContentGenerator;
        private readonly IOpenAPIImageGenerator _openAPIimageGenerator;
        private readonly BlogManager _blogManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="fnMainBot"/> class.
        /// </summary>
        /// <param name="openAPIContentGenerator">The content generator service.</param>
        /// <param name="openAPIimageGenerator">The image generator service.</param>
        /// <param name="blogManager">The blog manager instance.</param>
        public fnMainBot(IContentGenerator openAPIContentGenerator, IOpenAPIImageGenerator openAPIimageGenerator, BlogManager blogManager)
        {
            _openAPIContentGenerator = openAPIContentGenerator;
            _openAPIimageGenerator = openAPIimageGenerator;
            _blogManager = blogManager;
        }

        /// <summary>
        /// Timer trigger function to run the main bot.
        /// </summary>
        /// <param name="myTimer">The timer information.</param>
        [FunctionName("fnMainBot")]
        public async Task Run([TimerTrigger("%LookBeautyTimerSchedule%")] TimerInfo myTimer)
        {
            try
            {
                await _blogManager.RunAsync();
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }
    }
}



