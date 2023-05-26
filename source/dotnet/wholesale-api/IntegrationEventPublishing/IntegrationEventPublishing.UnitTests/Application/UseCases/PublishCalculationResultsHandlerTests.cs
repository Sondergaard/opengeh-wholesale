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
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.BatchAggregate;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.Processes;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.Processes.Model;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.UseCases;
using FluentAssertions;
using Moq;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.IntegrationEventPublishing.UnitTests.Application.UseCases;

public class PublishCalculationResultsHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task Foo(
        CompletedBatch completedBatch1,
        CompletedBatch completedBatch2,
        [Frozen] Mock<ICompletedBatchRepository> completedBatchRepositoryMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IProcessApplicationService> processApplicationServiceMock,
        PublishCalculationResultsHandler sut)
    {
        // Arrange
        completedBatch1.SetPrivateProperty(b => b.PublishedTime, null);
        completedBatch2.SetPrivateProperty(b => b.PublishedTime, null);

        completedBatchRepositoryMock
            .SetupSequence(repository => repository.GetNextUnpublishedOrNullAsync())
            .ReturnsAsync(completedBatch1)
            .ReturnsAsync(completedBatch2)
            .ReturnsAsync((CompletedBatch?)null);

        var expectedPublishCount = completedBatch1.GridAreaCodes.Count + completedBatch2.GridAreaCodes.Count;

        // Act
        await sut.PublishCalculationResultsAsync();

        // Assert

        // Batches completed
        completedBatch1.PublishedTime.Should().NotBeNull();
        completedBatch2.PublishedTime.Should().NotBeNull();

        // Publish invocation per grid area
        processApplicationServiceMock
            .Verify(service => service.PublishCalculationResultCompletedIntegrationEventsAsync(It.IsAny<ProcessCompletedEventDto>()), Times.Exactly(expectedPublishCount));

        // Unit of work commit per batch
        unitOfWorkMock
            .Verify(work => work.CommitAsync(), Times.Exactly(2));
    }
}
