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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResultClient.Mappers;

[UnitTest]
public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineData(TimeSeriesType.FlexConsumption, "flex_consumption")]
    [InlineData(TimeSeriesType.Production, "production")]
    [InlineData(TimeSeriesType.NonProfiledConsumption, "non_profiled_consumption")]
    public void ToDeltaTable_ReturnsExpectedString(TimeSeriesType type, string expected)
    {
        // Act
        var actual = TimeSeriesTypeMapper.ToDeltaTable(type);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("flex_consumption", TimeSeriesType.FlexConsumption)]
    [InlineData("production", TimeSeriesType.Production)]
    [InlineData("non_profiled_consumption", TimeSeriesType.NonProfiledConsumption)]
    public void FromDeltaTable_ReturnsExpectedString(string deltaTableValue, TimeSeriesType expected)
    {
        // Act
        var actual = TimeSeriesTypeMapper.FromDeltaTable(deltaTableValue);

        // Assert
        actual.Should().Be(expected);
    }
}
