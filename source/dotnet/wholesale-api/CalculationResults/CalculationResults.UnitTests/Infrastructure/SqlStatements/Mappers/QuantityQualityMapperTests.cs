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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using FluentAssertions;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements.Mappers;

public class QuantityQualityMapperTests
{
    [Fact]
    public async Task QuantityQuality_Matches_Contract()
    {
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.Enums.quantity-quality.json");
        await ContractComplianceTestHelper.VerifyEnumCompliesWithContractAsync<QuantityQuality>(stream);
    }

    [Fact]
    public void FromDeltaTableValue_ReturnsMappedQuality()
    {
        // Arrange
        foreach (var type in Enum.GetValues(typeof(QuantityQuality)))
        {
            var expected = (QuantityQuality)type;
            var input = expected.ToString().ToLower();

            // Act & Assert
            QuantityQualityMapper.FromDeltaTableValue(input).Should().Be(expected);
        }
    }

    [Fact]
    public async Task FromDeltaTableValue_MapsAllValidDeltaTableValues()
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.Enums.quantity-quality.json");
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        foreach (var validDeltaValue in validDeltaValues)
        {
            // Act
            var actual = QuantityQualityMapper.FromDeltaTableValue(validDeltaValue);

            // Assert it's a defined enum value (and not null)
            actual.Should().BeDefined();
        }
    }

    [Theory]
    [InlineAutoMoqData("calculated", QuantityQuality.Calculated)]
    [InlineAutoMoqData("estimated", QuantityQuality.Estimated)]
    [InlineAutoMoqData("incomplete", QuantityQuality.Incomplete)]
    [InlineAutoMoqData("measured", QuantityQuality.Measured)]
    [InlineAutoMoqData("missing", QuantityQuality.Missing)]
    public void FromDeltaTableValue_ReturnsValidQuantityQuality(string deltaValue, QuantityQuality? expected)
    {
        // Act
        var actual = QuantityQualityMapper.FromDeltaTableValue(deltaValue);

        // Assert
        actual.Should().Be(expected);
    }
}
