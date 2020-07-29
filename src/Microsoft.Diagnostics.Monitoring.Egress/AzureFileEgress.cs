using Microsoft.Diagnostics.Monitoring.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring
{
    public sealed class AzureFileEgress : IArtifactEgress
    {
        public async Task<string> UploadArtifact(string category, string name, Stream artifact, IProgress<long> progress, CancellationToken token)
        {
            string root = "/mnt/azure";
            if (!Directory.Exists(root))
            {
                return string.Empty;
            }

            string categoryPath = Path.Combine(root, category);
            if (!Directory.Exists(categoryPath))
            {
                Directory.CreateDirectory(categoryPath);
            }
            string fullPath = Path.Combine(categoryPath, name);

            using var outputDestination = File.Create(fullPath);
            await artifact.CopyToAsync(outputDestination, 81920, token);

            return fullPath;
        }
    }
}
