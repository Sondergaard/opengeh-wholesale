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

using System.Net;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Batches.Interfaces;
using Energinet.DataHub.Wholesale.Batches.Interfaces.Models;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;
using FluentAssertions;
using Moq;
using Test.Core;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi.V3;

public class ProcessStepResultTests : WebApiTestBase
{
    public ProcessStepResultTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, factory, testOutputHelper)
    {
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task HTTP_GET_V3_ReturnsHttpStatusCodeOkAtExpectedUrl(
        Mock<ICalculationResultClient> processStepResultRepositoryMock,
        Mock<IBatchApplicationService> batchApplicationServiceMock,
        ProcessStepResult result,
        Guid batchId,
        string gridAreaCode,
        BatchDto batchDto)
    {
        // Arrange
        result.SetPrivateProperty(r => r.TimeSeriesPoints, new TimeSeriesPoint[] { new(DateTimeOffset.Now, decimal.One, QuantityQuality.Measured) });
        processStepResultRepositoryMock
            .Setup(service => service.GetAsync(batchId, gridAreaCode, TimeSeriesType.Production, null, null))
            .ReturnsAsync(() => result);
        batchApplicationServiceMock.Setup(service => service.GetAsync(batchId)).ReturnsAsync(batchDto);
        Factory.ProcessStepResultRepositoryMock = processStepResultRepositoryMock;
        Factory.BatchApplicationServiceMock = batchApplicationServiceMock;

        var url = $"/v3/batches/{batchId}/processes/{gridAreaCode}/time-series-types/{TimeSeriesType.Production}";
        var expectedHttpStatusCode = HttpStatusCode.OK;

        // Act
        var actualContent = await Client.GetAsync(url);

        // Assert
        actualContent.StatusCode.Should().Be(expectedHttpStatusCode);
    }
}
