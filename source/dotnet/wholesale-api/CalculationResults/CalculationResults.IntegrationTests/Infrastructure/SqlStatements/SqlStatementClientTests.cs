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

using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;

public class SqlStatementClientTests
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly string _someSchemaName = $"TestSchema{Guid.NewGuid().ToString("N")[..8]}"; // TODO: use PR NUMBER
    private readonly string _sometableName = $"TestTable{Guid.NewGuid().ToString("N")[..8]}"; // TODO: use commit ID?
    private static readonly DatabricksOptions _databricksOptions = new()
    {
        DATABRICKS_WAREHOUSE_ID = "anyDatabricksId",
        DATABRICKS_WORKSPACE_URL = "https://anyDatabricksUrl",
        DATABRICKS_WORKSPACE_TOKEN = "myToken",
    };

    private readonly HttpClient _httpClient = CreateHttpClient();

    public SqlStatementClientTests()
    {
        await CreateSchemaAsync(_someSchemaName).ConfigureAwait(false);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteSqlStatementAsync_WhenQueryFromDatabricks_ReturnsExpectedData(
        [Frozen] Mock<IOptions<DatabricksOptions>> databricksOptionsMock)
    {
        // Arrange
        databricksOptionsMock.Setup(o => o.Value).Returns(_databricksOptions);
        var databricksSqlResponseParser = new DatabricksSqlResponseParser();
        var httpClient = new HttpClient();
        var sut = new SqlStatementClient(httpClient, databricksOptionsMock.Object, databricksSqlResponseParser);
        var sqlStatement = "SELECT * FROM myTable";

        // Act
        var response = await sut.ExecuteSqlStatementAsync(sqlStatement);

        // Assert
        response.RowCount.Should().Be(1);
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_databricksOptions.DATABRICKS_WORKSPACE_URL);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _databricksOptions.DATABRICKS_WORKSPACE_TOKEN);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        httpClient.BaseAddress = new Uri(_databricksOptions.DATABRICKS_WORKSPACE_URL);

        return httpClient;
    }

    private async Task CreateSchemaAsync(string schemaName)
    {
        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = @$"CREATE SCHEMA IF NOT EXISTS {schemaName}",
            warehouse_id = _databricksOptions.DATABRICKS_WAREHOUSE_ID,
        };

        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to create schema on Databricks. Status code: {response.StatusCode}");
    }

    private async Task CreateTableAsync(string schemaName, string tableName, Dictionary<string, string> columnNamesAndTypes)
    {
        var columnDefinitions = string.Join(", ", columnNamesAndTypes.Select(c => $"{c.Key} {c.Value}"));

        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = $@"CREATE TABLE {schemaName}.{tableName} ({columnDefinitions});",
            warehouse_id = _databricksOptions.DATABRICKS_WAREHOUSE_ID,
        };

        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DatabricksSqlException($"Unable to create table {schemaName}.{tableName} on Databricks. Status code: {response.StatusCode}");
    }
}
