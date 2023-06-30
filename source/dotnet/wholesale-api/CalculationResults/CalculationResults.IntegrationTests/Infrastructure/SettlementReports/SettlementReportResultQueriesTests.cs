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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.SettlementReports.Model;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Infrastructure.SettlementReports;

/// <summary>
/// We use an IClassFixture to control the life cycle of the DatabricksSqlStatementApiFixture so:
///   1. It is created and 'InitializeAsync()' is called before the first test in the test class is executed.
///      Use 'InitializeAsync()' to create any schema and seed data.
///   2. 'DisposeAsync()' is called after the last test in the test class has been executed.
///      Use 'DisposeAsync()' to drop any created schema.
/// </summary>
public class SettlementReportResultQueriesTests : IClassFixture<DatabricksSqlStatementApiFixture>, IAsyncLifetime
{
    private const string DefaultGridArea = "805";
    private const string SomeOtherGridArea = "111";
    private const ProcessType DefaultProcessType = ProcessType.BalanceFixing;
    private readonly DatabricksSqlStatementApiFixture _fixture;
    private readonly string[] _defaultGridAreaCodes = { DefaultGridArea };
    private readonly Instant _defaultPeriodStart = Instant.FromUtc(2022, 5, 16, 1, 0, 0);
    private readonly Instant _defaultPeriodEnd = Instant.FromUtc(2022, 5, 17, 1, 0, 0);

    public SettlementReportResultQueriesTests(DatabricksSqlStatementApiFixture fixture)
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

    [Theory]
    [InlineAutoMoqData]
    public async Task GetRowsAsync_ReturnsExpectedReportRow(Mock<ILogger<DatabricksSqlResponseParser>> loggerMock)
    {
        // Arrange
        var expectedSettlementReportRow = GetDefaultSettlementReportRow();
        var tableName = await CreateTableWithRowsFromMultipleBatches();
        var sqlStatementClient = new SqlStatementClient(new HttpClient(), _fixture.DatabricksOptionsMock.Object, new DatabricksSqlResponseParser(loggerMock.Object));
        var deltaTableOptions = CreateDeltaTableOptions(_fixture.DatabricksSchemaManager.SchemaName, tableName);
        var sut = new SettlementReportResultQueries(sqlStatementClient, deltaTableOptions);

        // Act
        var actual = await sut.GetRowsAsync(_defaultGridAreaCodes, DefaultProcessType, _defaultPeriodStart, _defaultPeriodEnd, null);

        // Assert
        var actualList = actual.ToList();
        actualList.Should().HaveCount(1);
        actualList.First().Should().Be(expectedSettlementReportRow);
    }

    private async Task<string> CreateTableWithRowsFromMultipleBatches()
    {
        var columnDefinitions = ResultDeltaTableHelper.GetColumnDefinitions();
        var tableName = await _fixture.DatabricksSchemaManager.CreateTableAsync(columnDefinitions);

        const string january1st = "2022-01-01T01:00:00.000Z";
        const string january2nd = "2022-01-02T01:00:00.000Z";
        const string january3rd = "2022-01-03T01:00:00.000Z";
        const string may1st = "2022-05-01T01:00:00.000Z";
        const string june1st = "2022-06-01T01:00:00.000Z";
        const string otherGridArea = "806";

        // Batch 1: Balance fixing, ExecutionTime=june1st, Period: 01/01 to 02/01 (include)
        var batch1Row1 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: may1st, time: january1st, batchProcessType: DeltaTableProcessType.BalanceFixing, gridArea: DefaultGridArea);
        var batch1Row2 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: may1st, time: january2nd, batchProcessType: DeltaTableProcessType.BalanceFixing, gridArea: DefaultGridArea);

        // Batch 2: Same as batch 1, but for other grid area (include)
        var batch2Row1 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: may1st, time: january1st, batchProcessType: DeltaTableProcessType.BalanceFixing, gridArea: otherGridArea);
        var batch2Row2 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: may1st, time: january2nd, batchProcessType: DeltaTableProcessType.BalanceFixing, gridArea: otherGridArea);

        // Batch 3: Same as batch 1, but only partly covering the same period (include)
        var batch3Row1 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: may1st, time: january2nd, batchProcessType: DeltaTableProcessType.BalanceFixing, gridArea: DefaultGridArea);
        var batch3Row2 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: may1st, time: january3rd, batchProcessType: DeltaTableProcessType.BalanceFixing, gridArea: DefaultGridArea);

        // Batch 4: Same as batch 1, but newer and for Aggregation (include)
        var batch4Row1 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: june1st, time: january1st, batchProcessType: DeltaTableProcessType.Aggregation, gridArea: DefaultGridArea);
        var batch4Row2 = ResultDeltaTableHelper.CreateRowValues(batchExecutionTimeStart: june1st, time: january2nd, batchProcessType: DeltaTableProcessType.Aggregation, gridArea: DefaultGridArea);

        var rows = new List<IEnumerable<string>> { batch1Row1, batch1Row2, batch2Row1, batch2Row2, batch3Row1, batch3Row2, batch4Row1, batch4Row2, };
        await _fixture.DatabricksSchemaManager.InsertIntoAsync(tableName, rows);

        return tableName;
    }

    private static SettlementReportResultRow GetDefaultSettlementReportRow()
    {
        return new SettlementReportResultRow(
            DefaultGridArea,
            ProcessType.BalanceFixing,
            Instant.FromUtc(2022, 5, 16, 3, 0, 0),
            "PT15M",
            MeteringPointType.Production,
            null,
            1.123m);
    }

    private static IOptions<DeltaTableOptions> CreateDeltaTableOptions(string schemaName, string tableName)
    {
        return Options.Create(new DeltaTableOptions { SCHEMA_NAME = schemaName, RESULT_TABLE_NAME = tableName, });
    }
}
