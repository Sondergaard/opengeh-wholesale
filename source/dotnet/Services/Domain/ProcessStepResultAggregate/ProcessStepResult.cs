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

namespace Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;

/// <summary>
/// Result data from a specific step in a process.
/// </summary>
public sealed class ProcessStepResult
{
    public ProcessStepResult(TimeSeriesType timeSeriesType, TimeSeriesPoint[] timeSeriesPoints)
    {
        if (timeSeriesPoints.Length == 0)
            throw new ArgumentException("Time series points empty");

        TimeSeriesType = timeSeriesType;
        TimeSeriesPoints = timeSeriesPoints;
        Min = timeSeriesPoints.Min(point => point.Quantity);
        Max = timeSeriesPoints.Max(point => point.Quantity);
        Sum = timeSeriesPoints.Sum(point => point.Quantity);
    }

    public decimal Sum { get; }

    public decimal Min { get; }

    public decimal Max { get; }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local - setter used in test
    public TimeSeriesType TimeSeriesType { get; private set; }

    public TimeSeriesPoint[] TimeSeriesPoints { get; }
}
