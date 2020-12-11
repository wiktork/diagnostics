﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.RestServer
{
    internal sealed class ApiAuthenticationOptions
    {
        public const string ConfigurationKey = "ApiAuthentication";

        public string ApiKeyHash { get; set; }
        public string HashType { get; set; }
    }
}
