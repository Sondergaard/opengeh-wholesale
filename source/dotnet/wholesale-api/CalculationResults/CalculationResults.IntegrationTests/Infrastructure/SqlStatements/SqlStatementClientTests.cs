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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SqlStatements;

/// <summary>
/// We use an IClassFixture to control the life cycle of the DatabricksSqlStatementApiFixture so:
///   1. It is created and 'InitializeAsync()' is called before the first test in the test class is executed.
///      Use 'InitializeAsync()' to create any schema and seed data.
///   2. 'DisposeAsync()' is called after the last test in the test class has been executed.
///      Use 'DisposeAsync()' to drop any created schema.
/// </summary>
public class SqlStatementClientTests : IClassFixture<DatabricksSqlStatementApiFixture>, IAsyncLifetime
{
    private readonly DatabricksSqlStatementApiFixture _fixture;

    public SqlStatementClientTests(DatabricksSqlStatementApiFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.DatabricksSchemaManager.CreateSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DatabricksSchemaManager.DropSchemaAsync();
    }

    private string SchemaName => _fixture.DatabricksSchemaManager.SchemaName;

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteSqlStatementAsync_WhenQueryFromDatabricks_ReturnsExpectedData(
        Mock<ILogger<DatabricksSqlStatusResponseParser>> loggerMock)
    {
        // Arrange
        var tableName = await CreateResultTableWithTwoRowsAsync();
        var sut = _fixture.CreateSqlStatementClient(loggerMock, new Mock<ILogger<SqlStatementClient>>());

        var sqlStatement = $@"SELECT * FROM {SchemaName}.{tableName}";

        // Act
        var actual = await sut.ExecuteAsync(sqlStatement).ToListAsync();

        // Assert
        actual.Count.Should().Be(2);
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteAsync_WhenMultipleChunks_ReturnsAllRows(Mock<ILogger<DatabricksSqlStatusResponseParser>> loggerMock)
    {
        // Arrange
        var expectedRowCount = 100;
        var sut = _fixture.CreateSqlStatementClient(loggerMock, new Mock<ILogger<SqlStatementClient>>());

        // Arrange: The result of this query spans multiple chunks
        var sqlStatement = $@"select r.id, 'some value' as value from range({expectedRowCount}) as r";

        // Act
        var actual = await sut.ExecuteAsync(sqlStatement).CountAsync();

        // Assert
        actual.Should().Be(expectedRowCount);
    }

    private async Task<string> CreateResultTableWithTwoRowsAsync()
    {
        var (someColumnDefinition, values) = GetSomeDeltaTableRow();

        var tableName = await _fixture.DatabricksSchemaManager.CreateTableAsync(someColumnDefinition);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, values);
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, values);

        return tableName;
    }

    private static (Dictionary<string, string> ColumnDefintion, List<string> Values) GetSomeDeltaTableRow()
    {
        var dictionary = new Dictionary<string, string>
        {
            { "someTimeColumn", "TIMESTAMP" },
            { "someStringColumn", "STRING" },
            { "someDecimalColumn", "DECIMAL(18,3)" },
        };

        var values = new List<string>
        {
            "'2022-03-11T03:00:00.000Z'",
            "'measured'",
            "1.234",
        };

        return (dictionary, values);
    }
}
