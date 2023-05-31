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
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using Xunit.Categories;
using static Moq.Protected.ItExpr;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResultClient;

[UnitTest]
public class CalculationResultClientTests
{
    private readonly string _anySqlStatement = "anySqlStatement";
    private readonly DatabricksOptions _someDatabricksOptions = new()
    {
        DATABRICKS_WAREHOUSE_ID = "anyDatabricksId",
        DATABRICKS_WORKSPACE_URL = "https://anyDatabricksUrl",
        DATABRICKS_WORKSPACE_TOKEN = "myToken",
    };

    private readonly DatabricksSqlResponse _cancelledDatabricksSqlResponse = DatabricksSqlResponse.CreateAsCancelled();
    private readonly DatabricksSqlResponse _pendingDatabricksSqlResponse = DatabricksSqlResponse.CreateAsPending();
    private readonly DatabricksSqlResponse _succeededDatabricksSqlResponse = DatabricksSqlResponse.CreateAsSucceeded(TableTestHelper.CreateTableForSettlementReport(3));
    private readonly DatabricksSqlResponse _succeededDatabricksSqlResponseWithZeroRows = DatabricksSqlResponse.CreateAsSucceeded(TableTestHelper.CreateTableForSettlementReport(0));
    private readonly DatabricksSqlResponse _failedDatabricksSqlResponse = DatabricksSqlResponse.CreateAsFailed();

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteSqlStatementAsync_WhenHttpStatusCodeNotOK_ThrowsDatabricksSqlException(
        [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, });
        var httpClient = new HttpClient(mockMessageHandler.Object);

        var sut = new SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<DatabricksSqlException>(() => sut.ExecuteSqlStatementAsync(_anySqlStatement));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteSqlStatementAsync_WhenDatabricksReturnsFailed_ThrowsDatabricksSqlException(
        [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.Setup(p => p.Parse(It.IsAny<string>())).Returns(_failedDatabricksSqlResponse);

        var sut = new SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<DatabricksSqlException>(() => sut.ExecuteSqlStatementAsync(_anySqlStatement));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteSqlStatementAsync_WhenDatabricksKeepsReturningPending_ThrowDatabricksSqlException(
        [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.Setup(p => p.Parse(It.IsAny<string>())).Returns(_pendingDatabricksSqlResponse);

        var sut = new SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<DatabricksSqlException>(() => sut.ExecuteSqlStatementAsync(_anySqlStatement));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteSqlStatementAsync_WhenDatabricksReturnsSucceeded_ReturnsExpectedNumberOfRows(
        [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(_succeededDatabricksSqlResponse);

        var sut = new SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act
        var actual = await sut.ExecuteSqlStatementAsync(_anySqlStatement);

        // Assert
        actual.RowCount.Should().Be(_succeededDatabricksSqlResponse.Table!.RowCount);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task
        ExecuteSqlStatementAsync_WhenDatabricksReturnsPendingAndThenSucceeded_ReturnsExpectedResponse(
            [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
            [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
            [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.SetupSequence(p => p.Parse(It.IsAny<string>()))
            .Returns(_pendingDatabricksSqlResponse).Returns(_succeededDatabricksSqlResponse);

        var sut = new SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act
        var actual = await sut.ExecuteSqlStatementAsync(_anySqlStatement);

        // Assert
        actual.RowCount.Should().Be(_succeededDatabricksSqlResponse.Table!.RowCount);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task
        ExecuteSqlStatementAsync_WhenDatabricksReturnsCancelledAndThenSucceeded_ReturnsExpectedResponse(
            [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
            [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
            [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.SetupSequence(p => p.Parse(It.IsAny<string>()))
            .Returns(_cancelledDatabricksSqlResponse).Returns(_succeededDatabricksSqlResponse);

        var sut = new SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act
        var actual = await sut.ExecuteSqlStatementAsync(_anySqlStatement);

        // Assert
        actual.RowCount.Should().Be(_succeededDatabricksSqlResponse.Table!.RowCount);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteSqlStatementAsync_WhenSuccessfulResponse_ReturnsExpectedNumberOfRows(
        [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(_succeededDatabricksSqlResponse);

        var sut = new SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act
        var actual = await sut.ExecuteSqlStatementAsync(_anySqlStatement);

        // Assert
        actual.RowCount.Should().Be(_succeededDatabricksSqlResponse.Table!.RowCount);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteSqlStatementAsync_WhenHttpEndpointReturnsRealSampleData_ReturnsExpectedData(
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        const int expectedRowCount = 96;
        const string expectedFirstQuantity = "0.000";
        const string expectedLastQuantity = "1.235";
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = GetValidHttpResponseContent(), });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        var sut = new SqlStatementClient(httpClient, mockOptions.Object, new DatabricksSqlResponseParser()); // here we use the real parser

        // Act
        var actual = await sut.ExecuteSqlStatementAsync(_anySqlStatement);

        // Assert
        actual.RowCount.Should().Be(expectedRowCount);
        actual[0, ResultColumnNames.Quantity].Should().Be(expectedFirstQuantity);
        actual[^1, ResultColumnNames.Quantity].Should().Be(expectedLastQuantity);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteSqlStatementAsync_WhenNoRelevantData_ReturnsZeroRows(
        [Frozen] Mock<IDatabricksSqlResponseParser> databricksSqlResponseParserMock,
        [Frozen] Mock<HttpMessageHandler> mockMessageHandler,
        [Frozen] Mock<IOptions<DatabricksOptions>> mockOptions)
    {
        // Arrange
        mockOptions.Setup(o => o.Value).Returns(_someDatabricksOptions);
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", IsAny<HttpRequestMessage>(), IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, });
        var httpClient = new HttpClient(mockMessageHandler.Object);
        databricksSqlResponseParserMock.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(_succeededDatabricksSqlResponseWithZeroRows);
        var sut = new SqlStatementClient(httpClient, mockOptions.Object, databricksSqlResponseParserMock.Object);

        // Act
        var actual = await sut.ExecuteSqlStatementAsync(_anySqlStatement);

        // Assert
        actual.RowCount.Should().Be(0);
    }

    private static StringContent GetValidHttpResponseContent()
    {
        var stream = EmbeddedResources.GetStream("Infrastructure.CalculationResultClient.CalculationResult.json");
        using var reader = new StreamReader(stream);
        return new StringContent(reader.ReadToEnd());
    }
}
