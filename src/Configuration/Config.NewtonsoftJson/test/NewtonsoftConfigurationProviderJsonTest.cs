// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration.NewtonsoftJson;
using Microsoft.Extensions.Configuration.Test;

namespace Microsoft.Extensions.Configuration
{
    public class ConfigurationProviderJsonTest : ConfigurationProviderTestBase
    {
        public override void Load_from_single_provider_with_duplicates_throws()
        {
            // JSON provider doesn't throw for duplicate values with the same case
            AssertConfig(BuildConfigRoot(LoadThroughProvider(TestSection.DuplicatesTestConfig)));
        }

        protected override (IConfigurationProvider Provider, Action Initializer) LoadThroughProvider(TestSection testConfig)
        {
            var jsonBuilder = new StringBuilder();
            SectionToJson(jsonBuilder, testConfig);

            var provider = new NewtonsoftJsonConfigurationProvider(
                new NewtonsoftJsonConfigurationSource
                {
                    Optional = true
                });

            var json = jsonBuilder.ToString();

            return (provider, () => provider.Load(TestStreamHelpers.StringToStream(json)));
        }

        private void SectionToJson(StringBuilder jsonBuilder, TestSection section)
        {
            string ValueToJson(object value) => value == null ? "null" : $"'{value}'";

            jsonBuilder.AppendLine("{");

            foreach (var tuple in section.Values)
            {
                jsonBuilder.AppendLine(tuple.Value.AsArray != null
                    ? $"'{tuple.Key}': [{string.Join(", ", tuple.Value.AsArray.Select(ValueToJson))}],"
                    : $"'{tuple.Key}': {ValueToJson(tuple.Value.AsString)},");
            }

            foreach (var tuple in section.Sections)
            {
                jsonBuilder.Append($"'{tuple.Key}': ");
                SectionToJson(jsonBuilder, tuple.Section);
            }

            jsonBuilder.AppendLine("},");
        }
    }
}