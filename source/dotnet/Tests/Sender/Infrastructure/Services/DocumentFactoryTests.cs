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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.Wholesale.Sender.Infrastructure;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Persistence.Processes;
using Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;
using FluentAssertions;
using Moq;
using NodaTime;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Sender.Infrastructure.Services;

[UnitTest]
public class DocumentFactoryTests
{
    private const string Expected = @"<?xml version=""1.0"" encoding=""utf-8""?>
<NotifyAggregatedMeasureData_MarketDocument xmlns=""urn:ediel.org:measure:notifyaggregatedmeasuredata:0:1"">
    <mRID>f0de3417-71ce-426e-9001-12600da9102a</mRID>
    <type>E31</type>
    <process.processType>D04</process.processType>
    <businessSector.type>23</businessSector.type>
    <sender_MarketParticipant.mRID codingScheme=""A10"">5790001330552</sender_MarketParticipant.mRID>
    <sender_MarketParticipant.marketRole.type>DGL</sender_MarketParticipant.marketRole.type>
    <receiver_MarketParticipant.mRID codingScheme=""A10"">8200000007739</receiver_MarketParticipant.mRID>
    <receiver_MarketParticipant.marketRole.type>MDR</receiver_MarketParticipant.marketRole.type>
    <createdDateTime>2022-07-04T08:05:30Z</createdDateTime>
    <Series>
        <mRID>1B8E673E-DBBD-4611-87A9-C7154937786A</mRID>
        <version>1</version>
        <marketEvaluationPoint.type>E18</marketEvaluationPoint.type>
        <meteringGridArea_Domain.mRID codingScheme=""NDK"">805</meteringGridArea_Domain.mRID>
        <product>8716867000030</product>
        <quantity_Measure_Unit.name>KWH</quantity_Measure_Unit.name>
        <Period>
            <resolution>PT15M</resolution>
            <timeInterval>
                <start>2022-06-30T22:00:00Z</start>
                <end>2022-07-01T22:00:00Z</end>
            </timeInterval>
            <Point>
                <position>1</position>
                <quantity>2.000</quantity>
                <quality>A04</quality>
            </Point>
        </Period>
    </Series>
</NotifyAggregatedMeasureData_MarketDocument>";

    /// <summary>
    /// Verifies the current completeness state of the document creation.
    /// </summary>
    [Theory]
    [InlineAutoMoqData]
    public async Task CreateAsync_ReturnsRsm014(
        DataBundleRequestDto request,
        Guid anyNotificationId,
        [Frozen] Mock<ICalculatedResultReader> calculatedResultReaderMock,
        [Frozen] Mock<IDocumentIdGenerator> documentIdGeneratorMock,
        [Frozen] Mock<ISeriesIdGenerator> seriesIdGeneratorMock,
        [Frozen] Mock<IProcessRepository> processRepositoryMock,
        [Frozen] Mock<IStorageHandler> storageHandlerMock,
        [Frozen] Mock<IClock> clockMock,
        DocumentFactory sut)
    {
        // Arrange
        documentIdGeneratorMock
            .Setup(generator => generator.Create())
            .Returns("f0de3417-71ce-426e-9001-12600da9102a");

        seriesIdGeneratorMock
            .Setup(generator => generator.Create())
            .Returns("1B8E673E-DBBD-4611-87A9-C7154937786A");

        var anyGridAreaCode = "805";
        processRepositoryMock
            .Setup(repository => repository.GetAsync(It.IsAny<MessageHubReference>()))
            .ReturnsAsync((MessageHubReference messageHubRef) => new Process(messageHubRef, anyGridAreaCode, Guid.NewGuid()));

        // TODO: This doesn't correspond with the integer quality from Spark and we're missing a Quality enum
        // TODO: Unit test all combinations of quality
        // TODO: How do we make sure that quantity will have 3 decimals in result and in RSM-014 too?
        var point = new PointDto(1, "2.000", Quality.Measured);

        calculatedResultReaderMock
            .Setup(x => x.ReadResultAsync(It.IsAny<Process>()))
            .ReturnsAsync(new BalanceFixingResultDto(new[] { point }));

        storageHandlerMock
            .Setup(handler => handler.GetDataAvailableNotificationIdsAsync(request))
            .ReturnsAsync(new[] { anyNotificationId });

        const string expectedIsoString = "2022-07-04T08:05:30Z";
        var instant = InstantPattern.General.Parse(expectedIsoString).Value;
        clockMock
            .Setup(clock => clock.GetCurrentInstant())
            .Returns(instant);

        using var outStream = new MemoryStream();

        // Act
        await sut.CreateAsync(request, outStream);
        outStream.Position = 0;

        // Assert
        using var stringReader = new StreamReader(outStream);
        var actual = await stringReader.ReadToEndAsync();

        actual.Replace("\r\n", "\n").Should().Be(Expected);
    }
}
