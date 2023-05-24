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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using FluentAssertions;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.DeltaTableContracts;

public class QuantityQualityMapperTests
{
    [Fact]
    public async Task QuantityQuality_Matches_Contract()
    {
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.quantity-quality.json");
        await ContractComplianceTestHelper.VerifyEnumCompliesWithContractAsync<QuantityQuality>(stream);
    }

    [Fact]
    public async Task QuantityQualityMapper_MapsAllValidDeltaTableValues()
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.quantity-quality.json");
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
    public void QuantityQualityMapper_ReturnsValidQuantityQuality(string deltaValue, QuantityQuality? expected)
    {
        // Act
        var actual = QuantityQualityMapper.FromDeltaTableValue(deltaValue);

        // Assert
        actual.Should().Be(expected);
    }
}
