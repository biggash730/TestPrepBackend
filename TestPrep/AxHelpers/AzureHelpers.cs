using System;
using System.IO;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

// Namespace for CloudConfigurationManager
// Namespace for CloudStorageAccount

// Namespace for Blob storage types

namespace TestPrep.AxHelpers
{
    public class AzureHelpers
    {
        static readonly CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(
    CloudConfigurationManager.GetSetting("StorageConnectionString"));
         
        public static string SaveImage(string content, string filename)
        {
            //save image to file
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data/Temp/" + filename);
            File.WriteAllBytes(filePath, Convert.FromBase64String(content));

            //read to blob
            var blobClient = StorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("pictures");
            container.CreateIfNotExists();
            var blockBlob = container.GetBlockBlobReference(filename);
            MemoryStream ms = new MemoryStream();
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(ms);
                blockBlob.UploadFromStream(ms);
            }
            //using (var fileStream = File.OpenRead(@filePath))
            return blockBlob.Uri.AbsoluteUri;
        }

        public static string SaveDocument(string content, string filename)
        {
            var blobClient = StorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("documents");
            container.CreateIfNotExists();
            container.SetPermissions(
                new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            // Retrieve reference to a blob named "myblob".
            var blockBlob = container.GetBlockBlobReference(filename);

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var fileStream = System.IO.File.OpenRead(@content))
            {
                blockBlob.UploadFromStream(fileStream);
            }
            return blockBlob.Uri.AbsoluteUri;
        }
    }
}