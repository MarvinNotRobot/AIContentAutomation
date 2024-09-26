using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace BlogHelper.function
{
    /// <summary>
    /// Azure Function class responsible for handling queue trigger events.
    /// </summary>
    public class QueueTriggerFunction
    {
        private ILogger<QueueTriggerFunction> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueTriggerFunction"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public QueueTriggerFunction(ILogger<QueueTriggerFunction> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Function triggered by messages in the specified Azure Storage Queue.
        /// </summary>
        /// <param name="myQueueItem">The queue item message.</param>
        [FunctionName("QueueTriggerFunction")]
        public void Run(
            [QueueTrigger("myqueue-items", Connection = "AzureWebJobsStorage")] string myQueueItem)
        {
            _logger?.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }

}
