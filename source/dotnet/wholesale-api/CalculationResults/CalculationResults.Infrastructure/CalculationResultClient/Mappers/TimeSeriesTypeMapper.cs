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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;

public static class TimeSeriesTypeMapper
{
    // These strings represents how write our results from spark to the delta table.
    // They should only be changed with changing how we write to the delta table.
    private const string NonProfiledConsumption = "non_profiled_consumption";
    private const string FlexConsumption = "flex_consumption";
    private const string Production = "production";
    private const string NetExchangePerGridArea = "net_exchange_per_ga";

    public static TimeSeriesType FromDeltaTableValue(string timeSeriesType) =>
        timeSeriesType switch
        {
            Production => TimeSeriesType.Production,
            FlexConsumption => TimeSeriesType.FlexConsumption,
            NonProfiledConsumption => TimeSeriesType.NonProfiledConsumption,
            NetExchangePerGridArea => TimeSeriesType.NetExchangePerGridArea,
            // TODO BJM
            "net_exchange_per_neighboring_ga" => TimeSeriesType.NetExchangePerNeighboringGa,
            "grid_loss" => TimeSeriesType.GridLoss,
            "negative_grid_loss" => TimeSeriesType.NegativeGridLoss,
            "positive_grid_loss" => TimeSeriesType.PositiveGridLoss,
            _ => throw new NotImplementedException($"Cannot map timeSeriesType type '{timeSeriesType}"),
        };

    public static string ToDeltaTableValue(TimeSeriesType timeSeriesType) =>
        timeSeriesType switch
        {
            TimeSeriesType.NonProfiledConsumption => NonProfiledConsumption,
            TimeSeriesType.Production => Production,
            TimeSeriesType.FlexConsumption => FlexConsumption,
            TimeSeriesType.NetExchangePerGridArea => NetExchangePerGridArea,
            _ => throw new NotImplementedException($"Mapping of '{timeSeriesType}' not implemented."),
        };
}
