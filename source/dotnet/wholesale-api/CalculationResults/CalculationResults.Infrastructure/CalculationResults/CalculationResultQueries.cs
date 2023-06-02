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

using System.Globalization;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults;

public class CalculationResultQueries : ICalculationResultQueries
{
    private readonly ISqlStatementClient _sqlStatementClient;

    public CalculationResultQueries(ISqlStatementClient sqlStatementClient)
    {
        _sqlStatementClient = sqlStatementClient;
    }

    public async Task<ProcessStepResult> GetAsync(
        Guid batchId,
        string gridAreaCode,
        TimeSeriesType timeSeriesType,
        string? energySupplierGln,
        string? balanceResponsiblePartyGln)
    {
        await Task.Delay(1000).ConfigureAwait(false);

        throw new NotImplementedException("GetAsync is not implemented yet");
    }

    private static ProcessStepResult CreateProcessStepResult(
        TimeSeriesType timeSeriesType,
        Table resultTable)
    {
        var pointsDto = Enumerable.Range(0, resultTable.RowCount)
            .Select(row => new TimeSeriesPoint(
                DateTimeOffset.Parse(resultTable[row, ResultColumnNames.Time]),
                decimal.Parse(resultTable[row, ResultColumnNames.Quantity], CultureInfo.InvariantCulture),
                QuantityQualityMapper.FromDeltaTableValue(resultTable[row, ResultColumnNames.QuantityQuality])))
            .ToList();

        return new ProcessStepResult(timeSeriesType, pointsDto.ToArray());
    }
}
