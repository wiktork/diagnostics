using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.RestServer
{
    internal static class AuthConstants
    {
        public const string PolicyName = "AuthorizedUserPolicy";
        public const string NegotiateSchema = "Negotiate";
        public const string NtlmSchema = "NTLM";
        public const string ApiKeySchema = "HashedKey";
    }
}
