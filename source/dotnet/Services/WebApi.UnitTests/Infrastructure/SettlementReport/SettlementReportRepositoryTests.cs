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

using System.Text;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.ProcessStep;
using Energinet.DataHub.Wholesale.Application.ProcessStep.Model;
using Energinet.DataHub.Wholesale.Domain.ActorAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Integration.DataLake;
using Energinet.DataHub.Wholesale.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.WebApi.UnitTests.Domain.BatchAggregate;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;
using TimeSeriesType = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.SettlementReport;

[UnitTest]
public class SettlementReportRepositoryTests
{
    private const string AnyActorId = "1234567890123";

    [Fact]
    public static async Task GetMasterBasisDataForTotalGridAreaFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.MasterBasisDataFileForTotalGridArea;

        // Act
        var actual =
            SettlementReportRepository.GetMasterBasisDataFileForTotalGridAreaSpecification(new Guid(batchId), new GridAreaCode(gridAreaCode));

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetTimeSeriesHourBasisDataForTotalGridAreaFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.TimeSeriesHourBasisDataFileForTotalGridArea;

        // Act
        var actual =
            SettlementReportRepository.GetTimeSeriesHourBasisDataForTotalGridAreaFileSpecification(
                new Guid(batchId),
                new GridAreaCode(gridAreaCode));

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetTimeSeriesQuarterBasisDataForTotalGridAreaFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.TimeSeriesQuarterBasisDataFileForTotalGridArea;

        // Act
        var actual =
            SettlementReportRepository.GetTimeSeriesQuarterBasisDataForTotalGridAreaFileSpecification(
                new Guid(batchId),
                new GridAreaCode(gridAreaCode));

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetMasterBasisDataForEsPerGaFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.MasterBasisDataFileForEsPerGa;

        // Act
        var actual =
            SettlementReportRepository.GetMasterBasisDataFileForForEsPerGaSpecification(new Guid(batchId), new GridAreaCode(gridAreaCode), AnyActorId);

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetTimeSeriesHourBasisDataForEsPerGaFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.TimeSeriesHourBasisDataFileForEsPerGa;

        // Act
        var actual =
            SettlementReportRepository.GetTimeSeriesHourBasisDataForEsPerGaGridAreaFileSpecification(
                new Guid(batchId),
                new GridAreaCode(gridAreaCode),
                AnyActorId);

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Fact]
    public static async Task GetTimeSeriesQuarterBasisDataForEsPerGaFileSpecification_MatchesContract()
    {
        // Arrange
        const string batchId = "eac4a18d-ed5f-46ba-bfe7-435ec0323519";
        const string gridAreaCode = "123";
        var calculationFilePathsContract = await CalculationFilePathsContract.GetAsync();
        var expected = calculationFilePathsContract.TimeSeriesQuarterBasisDataFileForEsPerGa;

        // Act
        var actual =
            SettlementReportRepository.GetTimeSeriesQuarterBasisDataForEsPerGaFileSpecification(
                new Guid(batchId),
                new GridAreaCode(gridAreaCode),
                AnyActorId);

        // Assert
        actual.Extension.Should().Be(expected.Extension);
        actual.Directory.Should().MatchRegex(expected.DirectoryExpression);
    }

    [Theory]
    [AutoMoqData]
    public async Task GetSettlementReportAsync_WhenGivenBatch_ReturnCorrectStream(
        [Frozen] Mock<IStreamZipper> streamZipperMock,
        [Frozen] Mock<IDataLakeClient> dataLakeFileClientMock)
    {
        // Arrange
        var basisDataBuffer = Encoding.UTF8.GetBytes("test");
        var memoryStream = new MemoryStream(basisDataBuffer);

        dataLakeFileClientMock
            .Setup(x => x.GetReadableFileStreamAsync(It.IsAny<string>()))
            .ReturnsAsync(memoryStream);

        var sut = new SettlementReportRepository(
            dataLakeFileClientMock.Object,
            streamZipperMock.Object);

        var batch = new BatchBuilder().Build();

        // Act
        var report = await sut.GetSettlementReportAsync(batch).ConfigureAwait(false);
        var actual = await new StreamReader(report.Stream).ReadLineAsync();

        // Assert
        actual.Should().Be("test");
    }

    [Theory]
    [AutoMoqData]
    public async Task GetSettlementReportAsync_WhenGivenBatchAndGridAreaCode_WritesToOutputStream(
        [Frozen] Mock<IStreamZipper> streamZipperMock,
        [Frozen] Mock<IDataLakeClient> dataLakeFileClientMock)
    {
        // Arrange
        const string expectedOutput = "test";
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedOutput));
        var batch = new BatchBuilder().Build();
        var gridAreaCode = new GridAreaCode("001");

        dataLakeFileClientMock.Setup(x => x.FindAndOpenFileAsync(It.IsAny<string>(), ".csv"))
            .ReturnsAsync(fileStream);

        using var outputStream = new MemoryStream();

        streamZipperMock.Setup(x =>
                // ReSharper disable AccessToDisposedClosure
                x.ZipAsync(
                    It.Is<IEnumerable<(Stream Stream, string Name)>>(files =>
                        files.All(file => file.Stream == fileStream && file.Name.StartsWith(gridAreaCode.Code))),
                    outputStream))
            .Callback(() =>
            {
                fileStream.CopyTo(outputStream);
                outputStream.Position = 0;
                // ReSharper restore AccessToDisposedClosure
            });

        var sut = new SettlementReportRepository(
            dataLakeFileClientMock.Object,
            streamZipperMock.Object);

        // Act
        await sut.GetSettlementReportAsync(batch, gridAreaCode, outputStream).ConfigureAwait(false);

        using var streamReader = new StreamReader(outputStream);
        var actual = await streamReader.ReadLineAsync();

        // Assert
        actual.Should().Be(expectedOutput);
    }

    [Theory]
    [AutoMoqData]
    public async Task CreateSettlementReportAsync_CreatesSettlementReportFile_WhenDataDirectoryIsNotFound(
        [Frozen] Mock<IStreamZipper> streamZipperMock,
        [Frozen] Mock<IDataLakeClient> dataLakeClientMock)
    {
        // Arrange
        var completedBatch = new BatchBuilder().Build();

        dataLakeClientMock.Setup(x => x.FindAndOpenFileAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new DataLakeDirectoryNotFoundException(string.Empty));

        var sut = new SettlementReportRepository(
            dataLakeClientMock.Object,
            streamZipperMock.Object);

        // Act & Assert
        await sut.Invoking(s => s.CreateSettlementReportsAsync(completedBatch)).Should().NotThrowAsync();
    }

    [Theory]
    [AutoMoqData]
    public async Task GetResultAsync_TimeSeriesPoint_IsRead(
        [Frozen] Mock<IProcessStepResultRepository> processActorResultRepositoryMock,
        [Frozen] Mock<IActorRepository> actorRepositoryMock)
    {
        // Arrange
        var time = new DateTimeOffset(2022, 05, 15, 22, 15, 0, TimeSpan.Zero);

        const decimal quantity = 1.000m;
        const string quality = "measured";
        const string gridAreaCode = "805";

        var batchId = Guid.NewGuid();

        var sut = new ProcessStepApplicationService(
            processActorResultRepositoryMock.Object,
            new ProcessStepResultMapper(),
            actorRepositoryMock.Object);

        processActorResultRepositoryMock.Setup(p => p.GetAsync(batchId, new GridAreaCode(gridAreaCode), TimeSeriesType.Production, null, null))
            .ReturnsAsync(new ProcessStepResult(TimeSeriesType.Production, new[] { new TimeSeriesPoint(time, quantity, quality) }));

        // Act
        var actual = await sut.GetResultAsync(
                batchId,
                gridAreaCode,
                Contracts.TimeSeriesType.Production,
                null,
                null);

        // Assert
        actual.TimeSeriesPoints.First().Time.Should().Be(time);
        actual.TimeSeriesPoints.First().Quantity.Should().Be(quantity);
        actual.TimeSeriesPoints.First().Quality.Should().Be(quality);
    }
}
