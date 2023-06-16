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
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Events.Application.CalculationResultPublishing.Model;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Factories;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Mappers;
using Energinet.DataHub.Wholesale.Events.Infrastructure.IntegrationEvents.Types;
using Energinet.DataHub.Wholesale.Events.UnitTests.Fixtures;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Xunit;
using ProcessType = Energinet.DataHub.Wholesale.Common.Models.ProcessType;
using QuantityQuality =
    Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.QuantityQuality;
using TimeSeriesPoint =
    Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesPoint;
using TimeSeriesType =
    Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Infrastructure.IntegrationEvents.Factories;

public class CalculationResultCompletedIntegrationEventFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void CreateCalculationResultCompletedForGridArea_WhenQuantityQualityCalculated_ExceptionIsThrown(
        CalculationResultCompletedIntegrationEventFactory sut,
        CalculationResultBuilder calculationResultBuilder)
    {
        // Arrange
        var calculationResult = calculationResultBuilder.Build();
        var batchGridAreaInfo = new BatchGridAreaInfo(
            "805",
            Guid.NewGuid(),
            ProcessType.Aggregation,
            Instant.FromUtc(2022, 5, 1, 0, 0),
            Instant.FromUtc(2022, 5, 2, 0, 0));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => sut.CreateForGridArea(
            calculationResult,
            batchGridAreaInfo));
    }

    [Theory]
    [InlineAutoMoqData]
    public void CreateCalculationResultCompletedForGridArea_WhenCreating_ResultIsForTotalGridArea(
        BatchGridAreaInfo anyBatchGridAreaInfo,
        CalculationResult anyCalculationResult,
        CalculationResultCompletedIntegrationEventFactory sut)
    {
        // Act
        var actual = sut.CreateForGridArea(
            anyCalculationResult,
            anyBatchGridAreaInfo);

        // Assert
        actual.AggregationPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerGridarea.Should().BeNull();
        actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
        actual.AggregationPerGridarea.Should().NotBeNull();
    }

    [Theory]
    [InlineAutoMoqData]
    public void CreateCalculationResultCompletedForGridArea_WhenCreating_PropertiesAreMappedCorrectly(
        BatchGridAreaInfo batchGridAreaInfo,
        CalculationResultBuilder calculationResultBuilder,
        CalculationResultCompletedIntegrationEventFactory sut)
    {
        // Arrange
        var timeSeriesPoint = new TimeSeriesPoint(DateTimeOffset.Now, 10.101000000m, QuantityQuality.Estimated);
        var calculationResult = calculationResultBuilder.WithTimeSeriesPoints(new[] { timeSeriesPoint }).Build();

        // Act
        var actual = sut.CreateForGridArea(
            calculationResult,
            batchGridAreaInfo);

        // Assert
        actual.AggregationPerGridarea.GridAreaCode.Should().Be(batchGridAreaInfo.GridAreaCode);
        actual.BatchId.Should().Be(batchGridAreaInfo.BatchId.ToString());
        actual.Resolution.Should().Be(Resolution.Quarter);
        actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
        actual.PeriodEndUtc.Should().Be(batchGridAreaInfo.PeriodEnd.ToTimestamp());
        actual.PeriodStartUtc.Should().Be(batchGridAreaInfo.PeriodStart.ToTimestamp());
        actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(calculationResult.TimeSeriesType));

        actual.TimeSeriesPoints[0].Quantity.Units.Should().Be(10);
        actual.TimeSeriesPoints[0].Quantity.Nanos.Should().Be(101000000);
        actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
        actual.TimeSeriesPoints[0].QuantityQuality.Should()
            .Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    }

    // [Theory]
    // [InlineAutoMoqData]
    // public void CreateCalculationResultCompletedForEnergySupplier_WhenCreating_ResultIsForTotalEnergySupplier(
    //     CalculationResult anyCalculationResult,
    //     BatchGridAreaInfo anyBatchGridAreaInfo,
    //     CalculationResultCompletedIntegrationEventFactory sut)
    // {
    //     // Act
    //     var actual = sut.CreateForEnergySupplier(
    //         anyCalculationResult,
    //         anyBatchGridAreaInfo,
    //         "AGlnNumber");
    //
    //     // Assert
    //     actual.AggregationPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
    //     actual.AggregationPerGridarea.Should().BeNull();
    //     actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
    //     actual.AggregationPerEnergysupplierPerGridarea.Should().NotBeNull();
    // }
    //
    // [Theory]
    // [InlineAutoMoqData]
    // public void CreateCalculationResultCompletedForEnergySupplier_WhenCreating_PropertiesAreMappedCorrectly(
    //     CalculationResult anyCalculationResult,
    //     BatchGridAreaInfo anyBatchGridAreaInfo,
    //     CalculationResultCompletedIntegrationEventFactory sut)
    // {
    //     // Arrange
    //     var expectedGln = "TheGln";
    //
    //     // Act
    //     var actual = sut.CreateForEnergySupplier(
    //         anyCalculationResult,
    //         anyBatchGridAreaInfo,
    //         expectedGln);
    //
    //     // Assert
    //     actual.AggregationPerEnergysupplierPerGridarea.GridAreaCode.Should().Be(anyBatchGridAreaInfo.GridAreaCode);
    //     actual.AggregationPerEnergysupplierPerGridarea.EnergySupplierGlnOrEic.Should().Be(expectedGln);
    //     actual.BatchId.Should().Be(anyBatchGridAreaInfo.BatchId.ToString());
    //     actual.Resolution.Should().Be(Resolution.Quarter);
    //     actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
    //     actual.PeriodEndUtc.Should().Be(anyBatchGridAreaInfo.PeriodEnd.ToTimestamp());
    //     actual.PeriodStartUtc.Should().Be(anyBatchGridAreaInfo.PeriodStart.ToTimestamp());
    //     actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(anyCalculationResult.TimeSeriesType));
    //
    //     actual.TimeSeriesPoints[0].Quantity.Units.Should().Be(10);
    //     actual.TimeSeriesPoints[0].Quantity.Nanos.Should().Be(101000000);
    //     actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
    //     actual.TimeSeriesPoints[0].QuantityQuality.Should()
    //         .Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    // }
    //
    // [Fact]
    // public void CreateCalculationResultCompletedForEnergySupplier_WhenQuantityQualityCalculated_ExceptionIsThrown()
    // {
    //     // Arrange
    //     var sut = new CalculationResultCompletedIntegrationEventFactory();
    //     var calculationResult = new CalculationResult(
    //         TimeSeriesType.Production,
    //         new TimeSeriesPoint[] { new(DateTimeOffset.Now, 10.0m, QuantityQuality.Calculated) });
    //     var batchGridAreaInfo = new BatchGridAreaInfo(
    //         "805",
    //         Guid.NewGuid(),
    //         ProcessType.Aggregation,
    //         Instant.FromUtc(2022, 5, 1, 0, 0),
    //         Instant.FromUtc(2022, 5, 2, 0, 0));
    //
    //     // Act & Assert
    //     Assert.Throws<ArgumentException>(() => sut.CreateForEnergySupplier(
    //         calculationResult,
    //         batchGridAreaInfo,
    //         "AGlnNumber"));
    // }
    //
    // [Fact]
    // public void CreateCalculationResultCompletedForBalanceResponsibleParty_ReturnsResultForBalanceResponsibleParty()
    // {
    //     // Arrange
    //     var sut = new CalculationResultCompletedIntegrationEventFactory();
    //     var calculationResult = new CalculationResult(
    //         TimeSeriesType.Production,
    //         new TimeSeriesPoint[] { new(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });
    //     var batchGridAreaInfo = new BatchGridAreaInfo(
    //         "805",
    //         Guid.NewGuid(),
    //         ProcessType.Aggregation,
    //         Instant.FromUtc(2022, 5, 1, 0, 0),
    //         Instant.FromUtc(2022, 5, 2, 0, 0));
    //
    //     // Act
    //     var actual = sut.CreateForBalanceResponsibleParty(
    //         calculationResult,
    //         batchGridAreaInfo,
    //         "ABrpGlnNumber");
    //
    //     // Assert
    //     actual.AggregationPerGridarea.Should().BeNull();
    //     actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
    //     actual.AggregationPerEnergysupplierPerGridarea.Should().BeNull();
    //     actual.AggregationPerBalanceresponsiblepartyPerGridarea.Should().NotBeNull();
    // }
    //
    // [Fact]
    // public void CreateCalculationResultCompletedForBalanceResponsibleParty_WhenCreating_PropertiesAreMappedCorrectly()
    // {
    //     // Arrange
    //     var sut = new CalculationResultCompletedIntegrationEventFactory();
    //     var timeSeriesPoint = new TimeSeriesPoint(DateTimeOffset.Now, 10.101000000m, QuantityQuality.Estimated);
    //     var calculationResult = new CalculationResult(
    //         TimeSeriesType.Production,
    //         new[] { timeSeriesPoint });
    //     var batchGridAreaInfo = new BatchGridAreaInfo(
    //         "805",
    //         Guid.NewGuid(),
    //         ProcessType.Aggregation,
    //         Instant.FromUtc(2022, 5, 1, 0, 0),
    //         Instant.FromUtc(2022, 5, 2, 0, 0));
    //     var expectedGln = "TheBrpGln";
    //
    //     // Act
    //     var actual = sut.CreateForBalanceResponsibleParty(
    //         calculationResult,
    //         batchGridAreaInfo,
    //         expectedGln);
    //
    //     // Assert
    //     actual.AggregationPerBalanceresponsiblepartyPerGridarea.GridAreaCode.Should()
    //         .Be(batchGridAreaInfo.GridAreaCode);
    //     actual.AggregationPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyGlnOrEic.Should()
    //         .Be(expectedGln);
    //     actual.BatchId.Should().Be(batchGridAreaInfo.BatchId.ToString());
    //     actual.Resolution.Should().Be(Resolution.Quarter);
    //     actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
    //     actual.PeriodEndUtc.Should().Be(batchGridAreaInfo.PeriodEnd.ToTimestamp());
    //     actual.PeriodStartUtc.Should().Be(batchGridAreaInfo.PeriodStart.ToTimestamp());
    //     actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(calculationResult.TimeSeriesType));
    //
    //     actual.TimeSeriesPoints[0].Quantity.Units.Should().Be(10);
    //     actual.TimeSeriesPoints[0].Quantity.Nanos.Should().Be(101000000);
    //     actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
    //     actual.TimeSeriesPoints[0].QuantityQuality.Should()
    //         .Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    // }
    //
    // [Fact]
    // public void
    //     CreateCalculationResultCompletedForBalanceResponsibleParty_WhenQuantityQualityCalculated_ExceptionIsThrown()
    // {
    //     // Arrange
    //     var sut = new CalculationResultCompletedIntegrationEventFactory();
    //     var calculationResult = new CalculationResult(
    //         TimeSeriesType.Production,
    //         new TimeSeriesPoint[] { new(DateTimeOffset.Now, 10.0m, QuantityQuality.Calculated) });
    //     var batchGridAreaInfo = new BatchGridAreaInfo(
    //         "805",
    //         Guid.NewGuid(),
    //         ProcessType.Aggregation,
    //         Instant.FromUtc(2022, 5, 1, 0, 0),
    //         Instant.FromUtc(2022, 5, 2, 0, 0));
    //
    //     // Act & Assert
    //     Assert.Throws<ArgumentException>(() => sut.CreateForBalanceResponsibleParty(
    //         calculationResult,
    //         batchGridAreaInfo,
    //         "ABrpGlnNumber"));
    // }
    //
    // [Fact]
    // public void
    //     CreateCalculationResultCompletedForEnergySupplierByBalanceResponsibleParty_ReturnsResultForEnergySupplierByBalanceResponsibleParty()
    // {
    //     // Arrange
    //     var sut = new CalculationResultCompletedIntegrationEventFactory();
    //     var calculationResult = new CalculationResult(
    //         TimeSeriesType.NonProfiledConsumption,
    //         new TimeSeriesPoint[] { new(DateTimeOffset.Now, 10.0m, QuantityQuality.Estimated) });
    //     var batchGridAreaInfo = new BatchGridAreaInfo(
    //         "805",
    //         Guid.NewGuid(),
    //         ProcessType.Aggregation,
    //         Instant.FromUtc(2022, 5, 1, 0, 0),
    //         Instant.FromUtc(2022, 5, 2, 0, 0));
    //
    //     // Act
    //     var actual = sut.CreateForEnergySupplierByBalanceResponsibleParty(
    //         calculationResult,
    //         batchGridAreaInfo,
    //         "AEsGlnNumber",
    //         "ABrpGlnNumber");
    //
    //     // Assert
    //     actual.AggregationPerGridarea.Should().BeNull();
    //     actual.AggregationPerEnergysupplierPerGridarea.Should().BeNull();
    //     actual.AggregationPerBalanceresponsiblepartyPerGridarea.Should().BeNull();
    //     actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.Should().NotBeNull();
    // }
    //
    // [Fact]
    // public void
    //     CreateCalculationResultCompletedForEnergySupplierByBalanceResponsibleParty_WhenCreating_PropertiesAreMappedCorrectly()
    // {
    //     // Arrange
    //     var sut = new CalculationResultCompletedIntegrationEventFactory();
    //     var timeSeriesPoint = new TimeSeriesPoint(DateTimeOffset.Now, 10.101000000m, QuantityQuality.Estimated);
    //     var calculationResult = new CalculationResult(
    //         TimeSeriesType.NonProfiledConsumption,
    //         new[] { timeSeriesPoint });
    //     var batchGridAreaInfo = new BatchGridAreaInfo(
    //         "805",
    //         Guid.NewGuid(),
    //         ProcessType.Aggregation,
    //         Instant.FromUtc(2022, 5, 1, 0, 0),
    //         Instant.FromUtc(2022, 5, 2, 0, 0));
    //     var expectedBrpGln = "TheBrpGln";
    //     var expectedEsGln = "TheEsGln";
    //
    //     // Act
    //     var actual = sut.CreateForEnergySupplierByBalanceResponsibleParty(
    //         calculationResult,
    //         batchGridAreaInfo,
    //         expectedEsGln,
    //         expectedBrpGln);
    //
    //     // Assert
    //     actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.GridAreaCode.Should()
    //         .Be(batchGridAreaInfo.GridAreaCode);
    //     actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.BalanceResponsiblePartyGlnOrEic
    //         .Should().Be(expectedBrpGln);
    //     actual.AggregationPerEnergysupplierPerBalanceresponsiblepartyPerGridarea.EnergySupplierGlnOrEic.Should()
    //         .Be(expectedEsGln);
    //     actual.BatchId.Should().Be(batchGridAreaInfo.BatchId.ToString());
    //     actual.Resolution.Should().Be(Resolution.Quarter);
    //     actual.QuantityUnit.Should().Be(QuantityUnit.Kwh);
    //     actual.PeriodEndUtc.Should().Be(batchGridAreaInfo.PeriodEnd.ToTimestamp());
    //     actual.PeriodStartUtc.Should().Be(batchGridAreaInfo.PeriodStart.ToTimestamp());
    //     actual.TimeSeriesType.Should().Be(TimeSeriesTypeMapper.MapTimeSeriesType(calculationResult.TimeSeriesType));
    //
    //     actual.TimeSeriesPoints[0].Quantity.Units.Should().Be(10);
    //     actual.TimeSeriesPoints[0].Quantity.Nanos.Should().Be(101000000);
    //     actual.TimeSeriesPoints[0].Time.Should().Be(timeSeriesPoint.Time.ToTimestamp());
    //     actual.TimeSeriesPoints[0].QuantityQuality.Should()
    //         .Be(QuantityQualityMapper.MapQuantityQuality(timeSeriesPoint.Quality));
    // }
    //
    // [Fact]
    // public void
    //     CreateCalculationResultCompletedForEnergySupplierByBalanceResponsibleParty_WhenQuantityQualityCalculated_ExceptionIsThrown()
    // {
    //     // Arrange
    //     var sut = new CalculationResultCompletedIntegrationEventFactory();
    //     var calculationResult = new CalculationResult(
    //         TimeSeriesType.NonProfiledConsumption,
    //         new TimeSeriesPoint[] { new(DateTimeOffset.Now, 10.0m, QuantityQuality.Calculated) });
    //     var batchGridAreaInfo = new BatchGridAreaInfo(
    //         "805",
    //         Guid.NewGuid(),
    //         ProcessType.Aggregation,
    //         Instant.FromUtc(2022, 5, 1, 0, 0),
    //         Instant.FromUtc(2022, 5, 2, 0, 0));
    //
    //     // Act & Assert
    //     Assert.Throws<ArgumentException>(() => sut.CreateForEnergySupplierByBalanceResponsibleParty(
    //         calculationResult,
    //         batchGridAreaInfo,
    //         "AEsGlnNumer",
    //         "ABrpGlnNumber"));
    // }
}
