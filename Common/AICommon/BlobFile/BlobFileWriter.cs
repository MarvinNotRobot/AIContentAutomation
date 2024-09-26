using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Common.Azure.Interface;


namespace Common.Azure.Blob
{
   
    public class AzureBlobStorageWriter : IStorageHandler
    {
        public async Task WriteToBlobStorage(string containerName, string blobName, IEnumerable<string> csvContent, bool append = false)
        {
            // Join CSV content
            string joinedCsvContent = string.Join("\n", csvContent);

            // Write CSV content to Azure Blob Storage
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=mykey;EndpointSuffix=core.windows.net";
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            containerClient.CreateIfNotExists();
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Append to existing blob content
            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(joinedCsvContent);
                writer.Flush();
                stream.Position = 0;
                await blobClient.UploadAsync(stream, overwrite: append);
            }
        }


        public  Task<string> GetBlobDownloadUrl(string connectionString, string containerName, string blobName)
        {
            
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            string sasToken = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1)).ToString();
            string downloadUrl = blobClient.Uri + "?" + sasToken;

            return Task.FromResult(downloadUrl);
        }

    }

}
