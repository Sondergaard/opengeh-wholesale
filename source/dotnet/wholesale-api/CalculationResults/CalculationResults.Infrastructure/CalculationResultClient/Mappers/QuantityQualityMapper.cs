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

public static class QuantityQualityMapper
{
        public static QuantityQuality FromDeltaTableValue(string pointQuality)
        {
            return pointQuality switch
            {
                "measured" => QuantityQuality.Measured,
                "calculated" => QuantityQuality.Calculated,
                "estimated" => QuantityQuality.Estimated,
                "incomplete" => QuantityQuality.Incomplete,
                "missing" => QuantityQuality.Missing,
                _ =>throw new ArgumentException($"quality of unknown type:{pointQuality}"),
            };
        }
}
