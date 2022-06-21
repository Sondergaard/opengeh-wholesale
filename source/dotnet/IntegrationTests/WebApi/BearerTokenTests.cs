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
using Energinet.DataHub.Wholesale.IntegrationTests.Core.Fixtures.WebApi;
using Energinet.DataHub.Wholesale.IntegrationTests.Core.TestCommon.WebApi;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.IntegrationTests.WebApi;

[Collection(nameof(BearerTokenTests))]
public class BearerTokenTests :
    WebApiTestBase<WholesaleWebApiFixture>,
    IClassFixture<WholesaleWebApiFixture>,
    IClassFixture<WebApiFactory>,
    IAsyncLifetime
{
    private const string BaseUrl = "/v1/batch";
    private const bool SuppliedJwtTokenIsValid = true;
    private const bool SuppliedJwtTokenIsInValid = false;
    private const string JwtBearerHttpHeader = "Authorization";
    private const string JwtBearerToken = "Bearer xxx";

    private readonly WebApiFactory _factory;

    public BearerTokenTests(
        WholesaleWebApiFixture wholesaleWebApiFixture,
        WebApiFactory factory,
        ITestOutputHelper testOutputHelper)
        : base(wholesaleWebApiFixture, testOutputHelper)
    {
        _factory = factory;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Request_missing_bearer_token_returns_401_Unauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove("Authorization");
        _factory.ReconfigureJwtTokenValidatorMock(SuppliedJwtTokenIsValid);

        // Act
        var response = await client.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Request_with_invalid_bearer_token_returns_401_Unauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(JwtBearerHttpHeader, JwtBearerToken);
        _factory.ReconfigureJwtTokenValidatorMock(SuppliedJwtTokenIsInValid);

        // Act
        var response = await client.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
