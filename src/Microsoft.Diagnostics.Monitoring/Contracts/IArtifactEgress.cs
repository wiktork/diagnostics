using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.Contracts
{
    public interface IArtifactEgress
    {
        Task UploadArtifact(string category, string name, Stream artifact, IProgress<long> progress, CancellationToken token);
    }
}
