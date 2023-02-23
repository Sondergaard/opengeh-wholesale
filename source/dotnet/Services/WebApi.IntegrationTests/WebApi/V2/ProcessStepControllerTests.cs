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
using System.Net.Http.Json;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Application.ProcessStep;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.WebApi;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.WebApi.V2;

public class ProcessStepControllerTests : WebApiTestBase
{
    public ProcessStepControllerTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, factory, testOutputHelper)
    {
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetActorsAsync_POST_V2_3_ReturnsExpectedResponse(
        Mock<IProcessStepApplicationService> applicationServiceMock,
        ProcessStepActorsRequest request,
        WholesaleActorDto expectedActor)
    {
        // Arrange
        const string expectedUrl = "/v2.3/ProcessStepResult";
        applicationServiceMock
            .Setup(service => service.GetActorsAsync(request))
            .ReturnsAsync(() => new[] { expectedActor });
        Factory.ProcessStepApplicationServiceMock = applicationServiceMock;

        // Act
        var actualContent = await Client.PostAsJsonAsync(expectedUrl, request);

        // Assert: Response HTTP status code
        actualContent.StatusCode.Should().Be(HttpStatusCode.OK);
        // Assert: Response body
        var actualActors = await actualContent.Content.ReadFromJsonAsync<List<WholesaleActorDto>>();
        actualActors!.Single().Should().BeEquivalentTo(expectedActor);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetResultAsync_POST_V2_ReturnsExpectedResponse(
        Mock<IProcessStepApplicationService> applicationServiceMock,
        ProcessStepResultRequestDto request,
        ProcessStepResultDto expectedProcessStepResult)
    {
        // Arrange
        applicationServiceMock
            .Setup(service => service.GetResultAsync(request.BatchId, request.GridAreaCode, TimeSeriesType.Production, null, null))
            .ReturnsAsync(() => expectedProcessStepResult);
        Factory.ProcessStepApplicationServiceMock = applicationServiceMock;

        // Act
        const string expectedUrl = "/v2/processstepresult";
        var actualContent = await Client.PostAsJsonAsync(expectedUrl, request);

        // Assert: Response HTTP status code
        actualContent.StatusCode.Should().Be(HttpStatusCode.OK);
        // Assert: Response body
        var actualActors = await actualContent.Content.ReadFromJsonAsync<ProcessStepResultDto>();
        actualActors.Should().BeEquivalentTo(expectedProcessStepResult);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetResultAsync_POST_V2_0_ReturnsExpectedResponse(
        Mock<IProcessStepApplicationService> applicationServiceMock,
        ProcessStepResultRequestDto request,
        ProcessStepResultDto expectedProcessStepResult)
    {
        // Arrange
        applicationServiceMock
            .Setup(service => service.GetResultAsync(request.BatchId, request.GridAreaCode, TimeSeriesType.Production, null, null))
            .ReturnsAsync(() => expectedProcessStepResult);
        Factory.ProcessStepApplicationServiceMock = applicationServiceMock;

        // Act
        const string expectedUrl = "/v2.0/processstepresult";
        var actualContent = await Client.PostAsJsonAsync(expectedUrl, request);

        // Assert: Response HTTP status code
        actualContent.StatusCode.Should().Be(HttpStatusCode.OK);
        // Assert: Response body
        var actualActors = await actualContent.Content.ReadFromJsonAsync<ProcessStepResultDto>();
        actualActors.Should().BeEquivalentTo(expectedProcessStepResult);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetResultAsync_POST_V2_1_ReturnsExpectedResponse(
        Mock<IProcessStepApplicationService> applicationServiceMock,
        ProcessStepResultRequestDto request,
        ProcessStepResultDto expectedProcessStepResult)
    {
        // Arrange
        applicationServiceMock
            .Setup(service => service.GetResultAsync(request.BatchId, request.GridAreaCode, TimeSeriesType.Production, null, null))
            .ReturnsAsync(() => expectedProcessStepResult);
        Factory.ProcessStepApplicationServiceMock = applicationServiceMock;

        // Act
        const string expectedUrl = "/v2.1/processstepresult";
        var actualContent = await Client.PostAsJsonAsync(expectedUrl, request);

        // Assert: Response HTTP status code
        actualContent.StatusCode.Should().Be(HttpStatusCode.OK);
        // Assert: Response body
        var actualActors = await actualContent.Content.ReadFromJsonAsync<ProcessStepResultDto>();
        actualActors.Should().BeEquivalentTo(expectedProcessStepResult);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetResultAsync_POST_V2_2_ReturnsExpectedResponse(
        Mock<IProcessStepApplicationService> applicationServiceMock,
        ProcessStepResultRequestDtoV2 request,
        ProcessStepResultDto expectedProcessStepResult)
    {
        // Arrange
        applicationServiceMock
            .Setup(service => service.GetResultAsync(request.BatchId, request.GridAreaCode, request.TimeSeriesType, request.Gln, null))
            .ReturnsAsync(() => expectedProcessStepResult);
        Factory.ProcessStepApplicationServiceMock = applicationServiceMock;

        // Act
        const string expectedUrl = "/v2.2/processstepresult";
        var actualContent = await Client.PostAsJsonAsync(expectedUrl, request);

        // Assert: Response HTTP status code
        actualContent.StatusCode.Should().Be(HttpStatusCode.OK);
        // Assert: Response body
        var actualActors = await actualContent.Content.ReadFromJsonAsync<ProcessStepResultDto>();
        actualActors.Should().BeEquivalentTo(expectedProcessStepResult);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task GetResultAsync_POST_V2_4_ReturnsExpectedResponse(
        Mock<IProcessStepApplicationService> applicationServiceMock,
        ProcessStepResultRequestDtoV3 request,
        ProcessStepResultDto expectedProcessStepResult)
    {
        // Arrange
        applicationServiceMock
            .Setup(service => service.GetResultAsync(request.BatchId, request.GridAreaCode, request.TimeSeriesType, request.EnergySupplierGln, request.BalanceResponsiblePartyGln))
            .ReturnsAsync(() => expectedProcessStepResult);
        Factory.ProcessStepApplicationServiceMock = applicationServiceMock;

        // Act
        const string expectedUrl = "/v2.4/processstepresult";
        var actualContent = await Client.PostAsJsonAsync(expectedUrl, request);

        // Assert: Response HTTP status code
        actualContent.StatusCode.Should().Be(HttpStatusCode.OK);
        // Assert: Response body
        var actualResult = await actualContent.Content.ReadFromJsonAsync<ProcessStepResultDto>();
        actualResult.Should().BeEquivalentTo(expectedProcessStepResult);
    }
}
