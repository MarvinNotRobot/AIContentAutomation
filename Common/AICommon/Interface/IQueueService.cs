using Azure.Storage.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Azure.Model;
using Azure.Storage.Queues.Models;

namespace Common.Azure.Interface
{
    public interface IQueueMessage
    {
        string id { get; }
        string Message { get; }
        int retry { get; }
        int LastException { get; }

    }
    public interface IQueueService
    {
        Task EnqueueAsync(string message);
        Task EnqueueAsync(IQueueMessage message);
    }
}
