// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Diagnostics.Monitoring.EventPipe.Triggers.Aspnet
{
    internal sealed class AspnetRequestStatusTriggerSettings : AspnetTriggerSettings
    {
        private const string StatusRegex = "[1-5][0-9]{2}";
        private static readonly string RangeRegex = FormattableString.Invariant($"{StatusRegex}(-{StatusRegex})?");
        private static readonly string StatusFilterRegex = FormattableString.Invariant($"^{RangeRegex}(;{RangeRegex})*$");

        /// <summary>
        /// Specifies the set of status codes for the trigger. This can be individual codes or ranges, delimited by semicolons.
        /// E.g. 200;400-500
        /// </summary>
        [Required]
        public string StatusCodeFilter { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = new List<ValidationResult>();

            if (!Regex.IsMatch(StatusCodeFilter, StatusFilterRegex))
            {
                results.Add(new ValidationResult($"{nameof(StatusCodeFilter)} is not in the correct format.",
                    new[] {nameof(StatusCodeFilter)}));
            }

            return results.Concat(base.Validate(validationContext));
        }
    }
}
