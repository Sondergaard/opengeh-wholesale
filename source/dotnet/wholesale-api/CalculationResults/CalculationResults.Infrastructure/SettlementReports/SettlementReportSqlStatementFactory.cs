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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.Common.Models;
using NodaTime;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SettlementReports;

public static class SettlementReportSqlStatementFactory
{
    public static string Create(
        string[] gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        string? energySupplier)
    {
        return string.IsNullOrEmpty(energySupplier) ? GetSqlForTotalGridAreas(gridAreaCodes, processType, periodStart, periodEnd) : GetSqlForEnergySupplier(gridAreaCodes, processType, periodStart, periodEnd, energySupplier);
    }

    private static string GetSqlForTotalGridAreas(
        string[] gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd)
    {
        var selectColumns = string.Join(
            ", ",
            @$"t1.{ResultColumnNames.GridArea}",
            @$"t1.{ResultColumnNames.BatchProcessType}",
            @$"t1.{ResultColumnNames.Time}",
            @$"t1.{ResultColumnNames.TimeSeriesType}",
            @$"t1.{ResultColumnNames.Quantity}");
        var processTypeString = ProcessTypeMapper.ToDeltaTableValue(processType);
        var gridAreas = string.Join(",", gridAreaCodes);
        var startTimeString = periodStart.ToString();
        var endTimeString = periodEnd.ToString();
        var timeSeriesTypes = new List<TimeSeriesType> { TimeSeriesType.Production, TimeSeriesType.FlexConsumption, TimeSeriesType.NonProfiledConsumption, TimeSeriesType.NetExchangePerGa };
        var timeSeriesTypesString = CreateTimeSeriesString(timeSeriesTypes);

        return $@"
SELECT {selectColumns}
FROM wholesale_output.result t1
LEFT JOIN wholesale_output.result t2
    ON t1.time = t2.time AND t1.batch_execution_time_start < t2.batch_execution_time_start
WHERE t2.time IS NULL
    AND t1.{ResultColumnNames.GridArea} IN ({gridAreas})
    AND t1.{ResultColumnNames.TimeSeriesType} IN ({timeSeriesTypesString})
    AND t1.{ResultColumnNames.BatchProcessType} = '{processTypeString}'
    AND t1.{ResultColumnNames.Time} BETWEEN '{startTimeString}' AND '{endTimeString}'
    AND t1.{ResultColumnNames.AggregationLevel} = '{DeltaTableAggregationLevel.GridArea}'
ORDER BY t1.time
";
    }

    private static string GetSqlForEnergySupplier(
        string[] gridAreaCodes,
        ProcessType processType,
        Instant periodStart,
        Instant periodEnd,
        string energySupplier)
    {
        var selectColumns = string.Join(
            ", ",
            @$"t1.{ResultColumnNames.GridArea}",
            @$"t1.{ResultColumnNames.BatchProcessType}",
            @$"t1.{ResultColumnNames.Time}",
            @$"t1.{ResultColumnNames.TimeSeriesType}",
            @$"t1.{ResultColumnNames.Quantity}");
        var processTypeString = ProcessTypeMapper.ToDeltaTableValue(processType);
        var gridAreas = string.Join(",", gridAreaCodes);
        var startTimeString = periodStart.ToString();
        var endTimeString = periodEnd.ToString();
        var timeSeriesTypes = new List<TimeSeriesType> { TimeSeriesType.Production, TimeSeriesType.FlexConsumption, TimeSeriesType.NonProfiledConsumption };
        var timeSeriesTypesString = CreateTimeSeriesString(timeSeriesTypes);

        return $@"
SELECT {selectColumns}
FROM wholesale_output.result t1
LEFT JOIN wholesale_output.result t2
    ON t1.time = t2.time AND t1.batch_execution_time_start < t2.batch_execution_time_start
WHERE t2.time IS NULL
    AND t1.{ResultColumnNames.GridArea} IN ({gridAreas})
    AND t1.{ResultColumnNames.TimeSeriesType} IN ({timeSeriesTypesString})
    AND t1.{ResultColumnNames.BatchProcessType} = '{processTypeString}'
    AND t1.{ResultColumnNames.Time} BETWEEN '{startTimeString}' AND '{endTimeString}'
    AND t1.{ResultColumnNames.AggregationLevel} = '{DeltaTableAggregationLevel.EnergySupplierAndGridArea}'
    AND t1.{ResultColumnNames.EnergySupplierId} = '{energySupplier}'
ORDER BY t1.time
";
    }

    private static string CreateTimeSeriesString(IEnumerable<TimeSeriesType> timeSeriesTypes)
    {
        var timeSeriesTypesString = string.Join(",", timeSeriesTypes.Select(x => $"\'{TimeSeriesTypeMapper.ToDeltaTableValue(x)}\'"));
        return timeSeriesTypesString;
    }
}
