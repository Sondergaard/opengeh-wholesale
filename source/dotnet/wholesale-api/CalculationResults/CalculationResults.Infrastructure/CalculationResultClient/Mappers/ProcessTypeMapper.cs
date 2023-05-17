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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResultClient.Mappers;

public static class ProcessTypeMapper
{
    public static string ToDeltaTableValue(Interfaces.CalculationResultClient.ProcessType processType) =>
        processType switch
        {
            Interfaces.CalculationResultClient.ProcessType.BalanceFixing => "BalanceFixing",
            Interfaces.CalculationResultClient.ProcessType.Aggregation => "Aggregation",
            _ => throw new NotImplementedException($"Cannot map process type '{processType}"),
        };

    public static Interfaces.CalculationResultClient.ProcessType FromDeltaTableValue(string processType) =>
        processType switch
        {
            "BalanceFixing" => Interfaces.CalculationResultClient.ProcessType.BalanceFixing,
            "Aggregation" => Interfaces.CalculationResultClient.ProcessType.Aggregation,
            _ => throw new NotImplementedException($"Cannot map process type '{processType}"),
        };
}
