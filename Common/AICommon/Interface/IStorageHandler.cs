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
    public interface IStorageHandler
    {
        public Task WriteToBlobStorage(string containerName, string blobName, IEnumerable<string> csvContent, bool append = false);
        public Task<string> GetBlobDownloadUrl(string connectionString, string containerName, string blobName);
    }

}
