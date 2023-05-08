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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using Energinet.DataHub.Wholesale.Infrastructure.Integration;
using FluentAssertions;
using Xunit;
using QuantityQuality = Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient.QuantityQuality;
using QuantityQualityMapper = Energinet.DataHub.Wholesale.Infrastructure.Integration.QuantityQualityMapper;
using TimeSeriesTypeMapper = Energinet.DataHub.Wholesale.Infrastructure.Integration.TimeSeriesTypeMapper;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.Integration;

public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineAutoMoqData(TimeSeriesType.Production, Wholesale.Contracts.Events.TimeSeriesType.Production)]
    [InlineAutoMoqData(TimeSeriesType.FlexConsumption, Wholesale.Contracts.Events.TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(TimeSeriesType.NonProfiledConsumption, Wholesale.Contracts.Events.TimeSeriesType.NonProfiledConsumption)]
    public void MapQuantityQuality_WhenCalled_MapsCorrectly(TimeSeriesType timeSeriesType, Wholesale.Contracts.Events.TimeSeriesType expected)
    {
        // Act & Assert
        TimeSeriesTypeMapper.MapTimeSeriesType(timeSeriesType).Should().Be(expected);
    }
}
