using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace massive_moose.storage.azure
{
    public class AzureFileStorage : IFileStorage
    {
        private CloudStorageAccount _storageAccount;
        private CloudBlobClient _blobClient;

        public AzureFileStorage(string connectionString)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            _blobClient = _storageAccount.CreateCloudBlobClient();
        }
        public void Delete(string filePath)
        {
            string[] filePathParts = filePath.Split('/');
            CloudBlobContainer container = _blobClient.GetContainerReference(filePathParts[0]);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePathParts[1]);
            blockBlob.Delete();
        }

        public bool Exists(string filePath)
        {
            string[] filePathParts = filePath.Split('/');
            CloudBlobContainer container = _blobClient.GetContainerReference(filePathParts[0]);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePathParts[1]);
            return blockBlob.Exists();
        }

        public byte[] Get(string filePath)
        {
            byte[] data = new byte[0];
            string[] filePathParts = filePath.Split('/');
            CloudBlobContainer container = _blobClient.GetContainerReference(filePathParts[0]);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePathParts[1]);
            using (var inStream = new MemoryStream())
            {
                blockBlob.DownloadToStream(inStream);
                return inStream.ToArray();
            }
        }

        public void Store(string filePath, byte[] data, bool overwrite = false)
        {
            string[] filePathParts = filePath.Split('/');
            CloudBlobContainer container = _blobClient.GetContainerReference(filePathParts[0]);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePathParts[1]);
            blockBlob.UploadFromByteArray(data, 0, data.Length);
        }
    }
}
