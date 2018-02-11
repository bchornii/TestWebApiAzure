using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TestWebApiAzure.BlobStores
{
    public class ImageStore
    {
        private readonly CloudBlobClient _client;
        private readonly Uri _baseUri = new Uri("https://testwebapistore.blob.core.windows.net/");
        public ImageStore()
        {
            _client = new CloudBlobClient(_baseUri, new StorageCredentials("testwebapistore", 
                "pHuPKdWxMfzmWvHA9XcooKvzrtUaLDswgZkGTLqdn15lJnf6sukra+NVPd45Cvl0GkwykWmL16qc5NGawlzbUQ=="));
        }

        public async Task<string> SaveImage(Stream stream)
        {
            var imageId = Guid.NewGuid().ToString();
            var container = _client.GetContainerReference("images");
            var blob = container.GetBlockBlobReference(imageId);
            await blob.UploadFromStreamAsync(stream);
            return imageId;
        }

        public async Task<IReadOnlyCollection<Uri>> GetBlobsUri()
        {
            var container = _client.GetContainerReference("images");
            var blobs = await container.ListBlobsSegmentedAsync(null);
            return blobs.Results.Select(b => b.Uri).ToArray();
        }

        public async Task CopyBlob(string sourceBlob, string targetBlob)
        {
            var container = _client.GetContainerReference("images");
            var source = container.GetBlockBlobReference(sourceBlob);
            var target = container.GetBlockBlobReference(targetBlob);
            await target.StartCopyAsync(source);
        }

        public Uri UriFor(string imageId)
        {
            var sasPolicy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTime.Now.AddMinutes(-15),
                SharedAccessExpiryTime = DateTime.Now.AddMinutes(30)
            };
            var container = _client.GetContainerReference("images");
            var blob = container.GetBlockBlobReference(imageId);
            var sasToken = blob.GetSharedAccessSignature(sasPolicy);
            return new Uri(_baseUri, $"/images/{imageId}{sasToken}");
        }

        private async Task SetMetadata(CloudBlobContainer container)
        {
            container.Metadata.Clear();
            container.Metadata.Add("Owner", "B.Chornii");
            await container.SetMetadataAsync();
        }

        private async Task<IDictionary<string, string>> GetMetadata(
            CloudBlobContainer container)
        {
            await container.FetchAttributesAsync();
            return container.Metadata;
        }
    }
}