using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TestWebApiAzure.BlobStores
{
    public class ImageBlobStore
    {
        private readonly CloudBlobClient _client;
        private readonly Uri _baseUri = new Uri("https://testwebapistore.blob.core.windows.net/");

        private CloudBlobContainer ImageContainer => _client.GetContainerReference("images");

        public ImageBlobStore()
        {
            _client = new CloudBlobClient(_baseUri, new StorageCredentials("testwebapistore", 
                "pHuPKdWxMfzmWvHA9XcooKvzrtUaLDswgZkGTLqdn15lJnf6sukra+NVPd45Cvl0GkwykWmL16qc5NGawlzbUQ=="));
        }

        public async Task<string> SaveImage(Stream stream)
        {
            var imageId = Guid.NewGuid().ToString();
            await ImageContainer.CreateIfNotExistsAsync();

            var blob = ImageContainer.GetBlockBlobReference(imageId);
            await blob.UploadFromStreamAsync(stream);
            return imageId;
        }

        public async Task<IReadOnlyCollection<string>> GetBlobsUri()
        {
            var blobs = await ImageContainer.ListBlobsSegmentedAsync(null);
            var sasToken = GetContainerSasToken();
            return blobs.Results
                .Select(b => b.Uri.ToString() + sasToken).ToArray();
        }        

        public Uri UriFor(string imageId)
        {
            var sasPolicy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTime.Now.AddMinutes(-15),
                SharedAccessExpiryTime = DateTime.Now.AddMinutes(30)
            };
            var blob = ImageContainer.GetBlockBlobReference(imageId);
            var sasToken = blob.GetSharedAccessSignature(sasPolicy);
            return new Uri(_baseUri, $"/images/{imageId}{sasToken}");
        }

        public async Task CopyBlob(string sourceBlob, string targetBlob)
        {
            var source = ImageContainer.GetBlockBlobReference(sourceBlob);
            var target = ImageContainer.GetBlockBlobReference(targetBlob);
            await target.StartCopyAsync(source);
        }

        private string GetContainerSasToken()
        {
            var sasPolicy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTime.Now.AddMinutes(-15),
                SharedAccessExpiryTime = DateTime.Now.AddMinutes(30)
            };
            return ImageContainer.GetSharedAccessSignature(sasPolicy);
        }

        private async Task SetMetadata()
        {
            ImageContainer.Metadata.Clear();
            ImageContainer.Metadata.Add("Owner", "B.Chornii");
            await ImageContainer.SetMetadataAsync();
        }

        private async Task<IDictionary<string, string>> GetMetadata()
        {
            await ImageContainer.FetchAttributesAsync();
            return ImageContainer.Metadata;
        }
    }
}