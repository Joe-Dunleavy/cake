// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake.Common.Tools.MSBuild
{
    internal static class MSBuildPropertyExtensions
    {
        private static readonly IReadOnlyDictionary<char, string> _escapeLookup = new Dictionary<char, string>
        {
            { ';', "%3B" },
            { ',', "%2C" },
            { ' ', "%20" },
            { '\r', "%0D" },
            { '\n', "%0A" }
        };

        private static readonly HashSet<string> _propertiesArgumentsNotEscapeSemiColons = new HashSet<string>
        {
            "DefineConstants",
            "ExcludeFilesFromDeployment"
        };

        internal static string BuildMSBuildPropertyParameterString(this KeyValuePair<string, IList<string>> property)
        {
            var propertyParameterString = new StringBuilder();
            var last = property.Value.Count - 1;
            var index = 0;

            var escapeSemiColons = property.Key.AllowEscapeSemiColon();
            foreach (var parameter in property.Value)
            {
                if (string.IsNullOrEmpty(parameter))
                {
                    index++;
                    continue;
                }

                propertyParameterString.Append(parameter.EscapeMSBuildPropertySpecialCharacters(escapeSemiColons));
                propertyParameterString.Append(index != last ? ";" : null);

                index++;
            }

            return propertyParameterString.ToString();
        }

        private static string EscapeMSBuildPropertySpecialCharacters(this string value, bool escapeSemiColons)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var escapedBuilder = new StringBuilder();
            foreach (var c in value)
            {
                if ((!escapeSemiColons && c.Equals(';')) || !_escapeLookup.TryGetValue(c, out var newChar))
                {
                    escapedBuilder.Append(c);
                }
                else
                {
                    escapedBuilder.Append(newChar);
                }
            }

            return escapedBuilder.ToString();
        }

        private static bool AllowEscapeSemiColon(this string propertyName)
        {
            return !_propertiesArgumentsNotEscapeSemiColons.Contains(propertyName);
        }
    }
}
