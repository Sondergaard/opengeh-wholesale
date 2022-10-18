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

using System.IO.Compression;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BasisData;
using Energinet.DataHub.Wholesale.IntegrationTests.Hosts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Test.Core;
using Xunit;

// TODO: Tests:
// - Creates zip without missing files
// - Logs error when files are missing
namespace Energinet.DataHub.Wholesale.IntegrationTests.ProcessManager;

// TODO: Why collection as a string? Why even a collection?
[Collection("ProcessManagerIntegrationTest")]
public sealed class BasisDataApplicationServiceTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task When_BatchIsCompleted_Then_CalculationFilesAreZipped(BatchCompletedEventDto batchCompletedEvent)
    {
        // Arrange
        var batch = CreateBatch(batchCompletedEvent);
        var serviceCollectionConfigurator = new ServiceCollectionConfigurator();
        var zipFileName = Path.GetTempFileName();

        using var host = await ProcessManagerIntegrationTestHost.CreateAsync(collection =>
            serviceCollectionConfigurator
                .WithBatchInDatabase(batch)
                .WithBasisDataFilesInCalculationStorage(batch, zipFileName)
                .Configure(collection));

        await using var scope = host.BeginScope();
        var sut = scope.ServiceProvider.GetRequiredService<IBasisDataApplicationService>();

        // Act
        await sut.ZipBasisDataAsync(batchCompletedEvent);

        // Assert
        // TODO: assert expected content and all files
        var zipExtractDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        ZipFile.ExtractToDirectory(zipFileName, zipExtractDirectory);
        var (directory, extension, zipEntryPath) = BatchFileManager.GetResultDirectory(batch.Id, batch.GridAreaCodes.Single());
        File.Exists(Path.Combine(zipExtractDirectory, zipEntryPath)).Should().BeTrue();
    }

    private static Batch CreateBatch(BatchCompletedEventDto batchCompletedEvent)
    {
        var gridAreaCode = new GridAreaCode("805");
        var batch = new Batch(
            ProcessType.BalanceFixing,
            new[] { gridAreaCode },
            SystemClock.Instance.GetCurrentInstant(),
            SystemClock.Instance.GetCurrentInstant(),
            SystemClock.Instance);
        batch.SetPrivateProperty(b => b.Id, batchCompletedEvent.BatchId);
        return batch;
    }
}
