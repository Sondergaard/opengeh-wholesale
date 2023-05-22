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

using System.Text;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient;

public static class SqlStatementFactory
{
    public static string CreateForSettlementReport(
        string[] gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        string? energySupplier)
    {
        // TODO: Handle energy supplier
        var selectColumns = string.Join(", ", ResultColumnNames.GridArea, ResultColumnNames.BatchProcessType, ResultColumnNames.Time, ResultColumnNames.TimeSeriesType, ResultColumnNames.Quantity);
        var timeSeriesTypes = string.Join(",", GetTimeSeriesTypesForSettlementReport().Select(x => $"\'{x}\'"));
        var processTypeString = ProcessTypeMapper.ToDeltaTableValue(processType);
        var gridAreas = string.Join(",", gridAreaCodes);
        var startTimeString = periodStart.ToString();
        var endTimeString = periodEnd.ToString();

        var sb = new StringBuilder();
        sb.Append($@"SELECT {selectColumns} ");
        sb.Append("FROM wholesale_output.result ");
        sb.Append($@"WHERE {ResultColumnNames.GridArea} IN ({gridAreas}) ");
        sb.Append($@"AND {ResultColumnNames.TimeSeriesType} IN ({timeSeriesTypes}) ");
        sb.Append($@"AND {ResultColumnNames.BatchProcessType} = '{processTypeString}' ");
        sb.Append($@"AND {ResultColumnNames.Time} BETWEEN '{startTimeString}' AND '{endTimeString}' ");
        sb.Append($@"AND {ResultColumnNames.AggregationLevel} = 'total_ga' ");
        sb.Append("ORDER by time");

        return sb.ToString();
    }

    private static IEnumerable<string> GetTimeSeriesTypesForSettlementReport()
    {
        return new List<string>
        {
            TimeSeriesTypeMapper.ToDeltaTableValue(TimeSeriesType.Production),
            TimeSeriesTypeMapper.ToDeltaTableValue(TimeSeriesType.FlexConsumption),
            TimeSeriesTypeMapper.ToDeltaTableValue(TimeSeriesType.NonProfiledConsumption),
            TimeSeriesTypeMapper.ToDeltaTableValue(TimeSeriesType.NetExchangePerGridArea),
        };
    }
}
