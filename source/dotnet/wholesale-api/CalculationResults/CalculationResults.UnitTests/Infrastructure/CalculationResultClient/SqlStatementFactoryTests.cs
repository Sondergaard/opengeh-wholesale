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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.DeltaTableConstants;
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResultClient;

[UnitTest]
public class SqlStatementFactoryTests
{
    [Fact]
    public void CreateForSettlementReport_ReturnsExpectedSqlStatement()
    {
        // Arrange
        var gridAreasCodes = new[] { "123", "234", "345" };
        var periodStart = Instant.FromUtc(2022, 10, 12, 1, 0);
        var periodEnd = Instant.FromUtc(2022, 10, 12, 3, 0);
        const string expectedSql = $@"
SELECT grid_area, batch_process_type, time, time_series_type, quantity
FROM wholesale_output.result
WHERE
    grid_area IN (123,234,345)
    AND time_series_type IN ('production','flex_consumption','non_profiled_consumption','net_exchange_per_ga')
    AND batch_process_type = 'BalanceFixing'
    AND time BETWEEN '2022-10-12T01:00:00Z' AND '2022-10-12T03:00:00Z'
    AND aggregation_level = 'total_ga'
ORDER by time
";

        // Act
        var actual = SqlStatementFactory.CreateForSettlementReport(gridAreasCodes, ProcessType.BalanceFixing, periodStart, periodEnd, null);

        // Assert
        actual.Should().Be(expectedSql);
    }
}
