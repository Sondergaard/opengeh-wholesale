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
using Energinet.DataHub.Wholesale.Calculations.Interfaces.Models;
using Energinet.DataHub.Wholesale.Events.Application.CompletedBatches;
using FluentAssertions;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Events.UnitTests.Application.UseCases.Factories;

public class CompletedBatchFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void CreateFromBatch_ReturnsCompletedBatch(CalculationDto calculation, CompletedBatchFactory sut)
    {
        // Arrange
        var expectedCompletedBatch = new CompletedBatch(
            calculation.BatchId,
            calculation.GridAreaCodes.ToList(),
            calculation.ProcessType,
            calculation.PeriodStart.ToInstant(),
            calculation.PeriodEnd.ToInstant(),
            calculation.ExecutionTimeEnd!.Value.ToInstant());

        // Act
        var actual = sut.CreateFromBatch(calculation);

        // Assert
        actual.Should().BeEquivalentTo(expectedCompletedBatch);
    }
}
