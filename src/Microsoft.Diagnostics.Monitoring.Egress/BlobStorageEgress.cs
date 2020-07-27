using Azure.Storage.Blobs;
using Microsoft.Diagnostics.Monitoring.Contracts;
using Microsoft.Extensions.Azure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring
{
    public sealed class BlobStorageEgress : IArtifactEgress
    {
        public BlobStorageEgress()
        {
        }

        public async Task UploadArtifact(string category, string name, Stream artifact, IProgress<long> progress, CancellationToken token)
        {
            string connectionInfo = "/etc/blobconnection/connection";
            BlobServiceClient serviceClient = null;
            if (File.Exists(connectionInfo))
            {
                serviceClient = new BlobServiceClient(File.ReadAllText(connectionInfo));
            }
            if (serviceClient == null)
            {
                return;
            }

            var blobContainerClient = serviceClient.GetBlobContainerClient(category);
            await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: token);

            var blobClient = blobContainerClient.GetBlobClient(name);
            var response = await blobClient.UploadAsync(artifact, cancellationToken: token, progressHandler: progress);
        }
    }
}
