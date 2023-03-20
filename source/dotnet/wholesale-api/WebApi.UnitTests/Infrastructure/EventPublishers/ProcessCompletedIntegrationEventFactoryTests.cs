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
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.EventPublishers;

public class IntegrationEventInfrastructureServiceTests
{
    [Theory]
    [AutoMoqData]
    public async Task AddAsync_WhenCreatingOutboxMessage(
        [Frozen] Mock<IOutboxMessageRepository> outboxMessageRepositoryMock,
        IntegrationEventDto integrationEventDto,
        IntegrationEventInfrastructureService sut)
    {
        // Arrange & Act
        await sut.AddAsync(integrationEventDto);

        // Assert
        outboxMessageRepositoryMock.Verify(x => x.AddAsync(It.Is<OutboxMessage>(message => message.MessageType == integrationEventDto.MessageType
            && message.Data == integrationEventDto.EventData
            && message.CreationDate == integrationEventDto.CreationDate)));
    }

    [Theory]
    [AutoMoqData]
    public async Task DeleteProcessedOlderThanAsync(
        [Frozen] Mock<IOutboxMessageRepository> outboxMessageRepositoryMock,
        [Frozen] Mock<IClock> clockMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventInfrastructureService sut)
    {
        // Arrange
        const int daysOld = 10;
        var instant = SystemClock.Instance.GetCurrentInstant();
        clockMock.Setup(x => x.GetCurrentInstant()).Returns(instant);

        // Act
        await sut.DeleteProcessedOlderThanAsync(daysOld);

        // Assert
        outboxMessageRepositoryMock.Verify(x => x.DeleteProcessedOlderThan(instant.Minus(Duration.FromDays(daysOld))));
        unitOfWorkMock.Verify(x => x.CommitAsync());
    }

    [Theory]
    [AutoMoqData]
    public async Task DispatchIntegrationEventsAsync(
        [Frozen] Mock<IIntegrationEventDispatcher> integrationEventDispatcherMock,
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        IntegrationEventInfrastructureService sut)
    {
        // Arrange
        integrationEventDispatcherMock.Setup(x => x.DispatchIntegrationEventsAsync(10)).ReturnsAsync(false);

        // Act
        await sut.DeleteProcessedOlderThanAsync(10);

        // Assert
        unitOfWorkMock.Setup(x => x.CommitAsync());
    }
}
