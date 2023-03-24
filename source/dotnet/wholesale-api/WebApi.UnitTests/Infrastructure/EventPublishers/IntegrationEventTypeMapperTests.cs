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
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Infrastructure.EventPublishers;

public class IntegrationEventTypeMapperTests
{
    [Theory]
    [AutoData]
    public void GetEventName_WhenEventType_ReturnsEventName(IntegrationEventTypeMapper sut)
    {
        // Arrange
        sut.Add(CalculationResultCompleted.BalanceFixingEventName, typeof(CalculationResultCompleted));
        const string expected = CalculationResultCompleted.BalanceFixingEventName;
        var eventType = typeof(CalculationResultCompleted);

        // Act
        var actual = sut.GetMessageType(eventType);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [AutoData]
    public void GetEventType_WhenEventName_ReturnsEventType(IntegrationEventTypeMapper sut)
    {
        // Arrange
        var type = typeof(string);

        // Act
        sut.Add("string", type);

        // Assert
        Assert.Equal(1, sut.Count());
    }

    [Theory]
    [AutoData]
    public void ThrowsException_WhenGettingNotExisting(IntegrationEventTypeMapper sut)
    {
        // Arrange
        var eventType = typeof(CalculationResultCompleted);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => sut.GetMessageType(eventType));
    }

    [Theory]
    [AutoData]
    public void ThrowsException_WhenAddingExistingEventType(IntegrationEventTypeMapper sut)
    {
        // Arrange
        const string eventName = "eventName";
        var eventType = typeof(CalculationResultCompleted);
        sut.Add(eventName, eventType);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => sut.Add(eventName, eventType));
    }

    [Theory]
    [AutoData]
    public void ThrowsException_WhenAddingExistingEventName(IntegrationEventTypeMapper sut)
    {
        // Arrange
        const string eventName = "eventName";
        var eventType = typeof(CalculationResultCompleted);
        sut.Add(eventName, eventType);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => sut.Add(eventName, typeof(string)));
    }
}
