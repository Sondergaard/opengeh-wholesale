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

using Energinet.DataHub.Wholesale.Calculations.Application.Model;
using Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;
using Energinet.DataHub.Wholesale.Common.Models;
using NodaTime;
using Test.Core;

namespace Energinet.DataHub.Wholesale.Calculations.UnitTests.Infrastructure.CalculationAggregate;

public class CalculationBuilder
{
    private readonly Instant _periodStart;
    private readonly Instant _periodEnd;

    private CalculationExecutionState? _state;
    private List<GridAreaCode> _gridAreaCodes = new() { new("805") };
    private ProcessType _processType = ProcessType.BalanceFixing;

    public CalculationBuilder()
    {
        // Create a valid period representing January in a +01:00 offset (e.g. time zone "Europe/Copenhagen")
        // In order to be valid the last millisecond must be omitted
        var firstOfJanuary = DateTimeOffset.Parse("2021-01-31T23:00Z");
        _periodStart = Instant.FromDateTimeOffset(firstOfJanuary);
        _periodEnd = Instant.FromDateTimeOffset(firstOfJanuary.AddMonths(1));
    }

    public CalculationBuilder WithStateSubmitted()
    {
        _state = CalculationExecutionState.Submitted;
        return this;
    }

    public CalculationBuilder WithStatePending()
    {
        _state = CalculationExecutionState.Pending;
        return this;
    }

    public CalculationBuilder WithStateExecuting()
    {
        _state = CalculationExecutionState.Executing;
        return this;
    }

    public CalculationBuilder WithStateCompleted()
    {
        _state = CalculationExecutionState.Completed;
        return this;
    }

    public CalculationBuilder WithGridAreaCode(string gridAreaCode)
    {
        _gridAreaCodes = new GridAreaCode(gridAreaCode).InList();
        return this;
    }

    public CalculationBuilder WithGridAreaCodes(List<GridAreaCode> gridAreaCodes)
    {
        _gridAreaCodes = gridAreaCodes;
        return this;
    }

    public CalculationBuilder WithProcessType(ProcessType processType)
    {
        _processType = processType;
        return this;
    }

    public Calculation Build()
    {
        var batch = new Calculation(
            SystemClock.Instance.GetCurrentInstant(),
            _processType,
            _gridAreaCodes,
            _periodStart,
            _periodEnd,
            SystemClock.Instance.GetCurrentInstant(),
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!,
            Guid.NewGuid());
        var jobRunId = new CalculationId(new Random().Next(1, 1000));

        if (_state == CalculationExecutionState.Submitted)
        {
            batch.MarkAsSubmitted(jobRunId);
        }
        else if (_state == CalculationExecutionState.Pending)
        {
            batch.MarkAsSubmitted(jobRunId);
            batch.MarkAsPending();
        }
        else if (_state == CalculationExecutionState.Executing)
        {
            batch.MarkAsSubmitted(jobRunId);
            batch.MarkAsPending();
            batch.MarkAsExecuting();
        }
        else if (_state == CalculationExecutionState.Completed)
        {
            batch.MarkAsSubmitted(jobRunId);
            batch.MarkAsPending();
            batch.MarkAsExecuting();
            batch.MarkAsCompleted(SystemClock.Instance.GetCurrentInstant());
        }
        else if (_state != null)
        {
            throw new NotImplementedException();
        }

        return batch;
    }
}
