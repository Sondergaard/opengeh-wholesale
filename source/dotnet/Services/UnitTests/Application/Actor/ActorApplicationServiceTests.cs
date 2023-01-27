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
using Energinet.DataHub.Wholesale.Application.BatchActor;
using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.Actor;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Tests.Domain.BatchAggregate;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;
using MarketRoleType = Energinet.DataHub.Wholesale.Domain.Actor.MarketRoleType;
using TimeSeriesType = Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate.TimeSeriesType;

namespace Energinet.DataHub.Wholesale.Tests.Application.Actor;

[UnitTest]
public class ActorApplicationServiceTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task GetAsync_ReturnsBatchActorDtos(
        Guid batchId,
        [Frozen] Mock<IActorRepository> actorRepositoryMock,
        ActorApplicationService sut)
    {
        // Arrange
        var actorsRequest = new ProcessStepActorsRequest(
            batchId,
            "805",
            Contracts.TimeSeriesType.Production,
            Contracts.MarketRoleType.EnergySupplier);

        var glnNumber = "AnyGlnNumber";
        actorRepositoryMock
            .Setup(x => x.GetAsync(
                actorsRequest.BatchId,
                new GridAreaCode(actorsRequest.GridAreaCode),
                TimeSeriesType.Production,
                MarketRoleType.EnergySupplier)).ReturnsAsync(new Wholesale.Domain.Actor.Actor[] { new(glnNumber) });

        // Act
        var batchActorDtos = await sut.GetAsync(actorsRequest);

        // Assert
        batchActorDtos.First().Gln.Should().Be(glnNumber);
    }
}
