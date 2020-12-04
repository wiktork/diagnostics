using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.RestServer
{
    internal class AuthOptions
    {
        public bool AuthRequired { get; set; }

        public string ApiKey { get; set; }
        
    }
}
