using Azure.Storage.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Azure.Model
{
    public class QueueConfig
    {
        public string storageConnectionString;
        public string queueName;
    }

}
