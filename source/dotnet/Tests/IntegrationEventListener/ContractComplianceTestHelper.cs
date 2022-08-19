﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using FluentAssertions;
using Newtonsoft.Json;

namespace Energinet.DataHub.Wholesale.Tests.IntegrationEventListener;

internal static class ContractComplianceTestHelper
{
    public static async Task<string> GetRequiredMessageTypeAsync(Stream contractStream)
    {
        using var streamReader = new StreamReader(contractStream);
        var contractJson = await streamReader.ReadToEndAsync();
        var contractDescription = JsonConvert.DeserializeObject<dynamic>(contractJson)!;

        foreach (var fieldDescriptor in contractDescription.bodyFields)
        {
            if (fieldDescriptor.name == "MessageType")
            {
                return fieldDescriptor.value;
            }
        }

        throw new InvalidOperationException("Could not find required MessageType in contract.");
    }

    public static async Task VerifyTypeCompliesWithContractAsync<T>(Stream contractStream)
    {
        using var streamReader = new StreamReader(contractStream);
        var contractJson = await streamReader.ReadToEndAsync();
        var contractDescription = JsonConvert.DeserializeObject<dynamic>(contractJson)!;

        var expectedProps = contractDescription.bodyFields;
        var actualProps = typeof(T)
            .GetProperties()
            .ToDictionary(info => info.Name);

        // Assert: Number of props match
        actualProps.Count.Should().Be(expectedProps.Count);

        foreach (var expectedProp in expectedProps)
        {
            string expectedPropName = expectedProp.name;
            string expectedPropType = expectedProp.type;

            // Assert: Lookup property by name
            var actualProp = actualProps[expectedPropName];

            // Assert: Property types match
            var actualPropertyType = MapToContractType(actualProp.PropertyType);
            actualPropertyType.Should().Be(expectedPropType);
        }
    }

    private static string MapToContractType(Type propertyType)
    {
        if (propertyType.IsEnum)
            return MapToContractType(Enum.GetUnderlyingType(propertyType));

        if (Nullable.GetUnderlyingType(propertyType) is { } underlyingType)
            return MapToContractType(underlyingType);

        return propertyType.Name switch
        {
            "Int32" => "integer",
            "String" => "string",
            "Guid" => "string",
            "Instant" => "timestamp",
            _ => throw new NotImplementedException($"Property type '{propertyType.Name}' not implemented."),
        };
    }
}
