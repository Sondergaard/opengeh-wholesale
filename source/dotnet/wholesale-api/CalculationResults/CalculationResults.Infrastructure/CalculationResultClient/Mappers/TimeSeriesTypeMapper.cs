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
    public static TimeSeriesType FromDeltaTableValue(string timeSeriesType) =>
        timeSeriesType switch
        {
            "production" => TimeSeriesType.Production,
            "flex_consumption" => TimeSeriesType.FlexConsumption,
            "non_profiled_consumption" => TimeSeriesType.NonProfiledConsumption,
            "net_exchange_per_ga" => TimeSeriesType.ExchangePerGridArea,
            _ => throw new NotImplementedException($"Cannot map timeSeriesType type '{timeSeriesType}"),
        };

    public static string ToDeltaTableValue(TimeSeriesType timeSeriesType) =>
        timeSeriesType switch
        {
            TimeSeriesType.NonProfiledConsumption => "non_profiled_consumption",
            TimeSeriesType.Production => "production",
            TimeSeriesType.FlexConsumption => "flex_consumption",
            TimeSeriesType.ExchangePerGridArea => "net_exchange_per_ga",
            _ => throw new NotImplementedException($"Mapping of '{timeSeriesType}' not implemented."),
        };
}
