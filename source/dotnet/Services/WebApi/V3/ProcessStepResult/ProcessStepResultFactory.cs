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

using Energinet.DataHub.Wholesale.Application.Batches.Model;

namespace Energinet.DataHub.Wholesale.WebApi.V3.ProcessStepResult;

public class ProcessStepResultFactory : IProcessStepResultFactory
{
    public ProcessStepResultDto Create(Contracts.ProcessStepResultDto stepResult, BatchDto batch)
    {
        return new ProcessStepResultDto(
            stepResult.Sum,
            stepResult.Min,
            stepResult.Max,
            batch.PeriodStart,
            batch.PeriodEnd,
            batch.Resolution,
            batch.Unit,
            stepResult.TimeSeriesPoints.Select(Map()).ToArray());
    }

    private static Func<Contracts.TimeSeriesPointDto, TimeSeriesPointDto> Map()
    {
        return p => new TimeSeriesPointDto(p.Time, p.Quantity, Map(p.Quality));
    }

    private static string Map(string quality) =>
        // TODO: Seems like we are missing a contract for these (CIM) qualities between .NET and pyspark
        // TODO: Should we stop using these CIM names?
        quality switch
        {
            "A02" => TimeSeriesPointQuality.Missing,
            "A03" => TimeSeriesPointQuality.Estimated,
            "A04" => TimeSeriesPointQuality.Measured,
            "A06" => TimeSeriesPointQuality.Calculated,
            _ => throw new NotImplementedException($"No mapping defined for quality '{quality}'"),
        };
}
