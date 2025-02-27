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
using Energinet.DataHub.Wholesale.Batches.Application.Model;
using Energinet.DataHub.Wholesale.Batches.Application.Model.Batches;
using Energinet.DataHub.Wholesale.Batches.Infrastructure.Calculations;
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using Microsoft.Azure.Databricks.Client;
using Microsoft.Azure.Databricks.Client.Models;
using NodaTime;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Infrastructure.Calculations;

public class DatabricksCalculatorJobParametersFactoryTests
{
    [Theory]
    [InlineAutoMoqData]
    public void CreateParameters_MatchesExpectationOfDatabricksJob(
        DatabricksCalculationParametersFactory sut)
    {
        // Arrange
        var batch = new Batch(
            SystemClock.Instance.GetCurrentInstant(),
            ProcessType.BalanceFixing,
            new List<GridAreaCode> { new("805"), new("806"), new("033") },
            DateTimeOffset.Parse("2022-05-31T22:00Z").ToInstant(),
            DateTimeOffset.Parse("2022-06-01T22:00Z").ToInstant(),
            DateTimeOffset.Parse("2022-06-04T22:00Z").ToInstant(),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!,
            Guid.NewGuid());

        using var stream = EmbeddedResources.GetStream("Infrastructure.Calculations.calculation-job-parameters-reference.txt");
        using var reader = new StreamReader(stream);

        var pythonParams = reader
            .ReadToEnd()
            .Replace("{batch-id}", batch.Id.ToString())
            .Replace("\r", string.Empty)
            .Split("\n") // Split lines
            .Where(l => !l.StartsWith("#") && l.Length > 0); // Remove empty and comment lines
        var expected = RunParameters.CreatePythonParams(pythonParams);

        // Act
        var actual = sut.CreateParameters(batch);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }
}
