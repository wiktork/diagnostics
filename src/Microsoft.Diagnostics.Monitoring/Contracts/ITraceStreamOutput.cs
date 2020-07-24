using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.Contracts
{
    public interface ITraceStreamOutput
    {
        Task EventStreamAvailable(Stream eventStream);
    }
}
