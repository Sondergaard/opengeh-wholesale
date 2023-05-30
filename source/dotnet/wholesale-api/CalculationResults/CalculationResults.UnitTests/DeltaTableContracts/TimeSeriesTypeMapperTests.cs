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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using FluentAssertions;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.DeltaTableContracts;

public class TimeSeriesTypeMapperTests
{
    [Fact]
    public async Task TimeSeriesType_Matches_Contract()
    {
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.Enums.time-series-type.json");
        await ContractComplianceTestHelper.VerifyEnumCompliesWithContractAsync<TimeSeriesType>(stream);
    }

    [Fact]
    public async Task TimeSeriesTypeMapper_MapsAllValidDeltaTableValues()
    {
        // Arrange
        await using var stream = EmbeddedResources.GetStream("DeltaTableContracts.Contracts.Enums.time-series-type.json");
        var validDeltaValues = await ContractComplianceTestHelper.GetCodeListValuesAsync(stream);

        foreach (var validDeltaValue in validDeltaValues)
        {
            // Act
            var actual = TimeSeriesTypeMapper.FromDeltaTableValue(validDeltaValue);

            // Assert it's a defined enum value (and not null)
            actual.Should().BeDefined();
        }
    }

    [Theory]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.Production, TimeSeriesType.Production)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NonProfiledConsumption, TimeSeriesType.NonProfiledConsumption)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NetExchangePerNeighboringGridArea, TimeSeriesType.NetExchangePerNeighboringGa)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NetExchangePerGridArea, TimeSeriesType.NetExchangePerGa)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.FlexConsumption, TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.GridLoss, TimeSeriesType.GridLoss)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NegativeGridLoss, TimeSeriesType.NegativeGridLoss)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.PositiveGridLoss, TimeSeriesType.PositiveGridLoss)]
    public void TimeSeriesTypeMapper_ReturnsValidTimeSeriesType(string deltaValue, TimeSeriesType? expected)
    {
        // Act
        var actual = TimeSeriesTypeMapper.FromDeltaTableValue(deltaValue);

        // Assert
        actual.Should().Be(expected);
    }
}
