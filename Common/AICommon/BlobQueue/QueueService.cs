using Azure.Storage.Queues;
using Common.Azure.Model;
using Common.Azure.Interface;


namespace Common.Azure.Blob
{ 
 
    public class QueueService : IQueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(QueueConfig config)
        {
            _queueClient = new QueueClient(config.storageConnectionString, config.queueName);
        }

        public Task  EnqueueAsync(IQueueMessage message)
        {
            throw new NotImplementedException("EnqueueAsync(QueueMessage message) not implemented");
        }

        public async Task EnqueueAsync(string message)
        {

            if (!await _queueClient.ExistsAsync())
            {
                await _queueClient.CreateAsync();
            }

            await _queueClient.SendMessageAsync(message);
        }
    }

}
