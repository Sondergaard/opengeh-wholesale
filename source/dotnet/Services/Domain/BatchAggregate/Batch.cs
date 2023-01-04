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

using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Domain.BatchAggregate;

public class Batch
{
    private readonly List<GridAreaCode> _gridAreaCodes;
    private readonly IClock _clock;

    public Batch(ProcessType processType, IEnumerable<GridAreaCode> gridAreaCodes, Instant periodStart, Instant periodEnd, IClock clock)
        : this()
    {
        ExecutionState = BatchExecutionState.Created;
        ProcessType = processType;
        _clock = clock;
        _gridAreaCodes = gridAreaCodes.ToList();
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        ExecutionTimeStart = _clock.GetCurrentInstant();
        ExecutionTimeEnd = null;
        IsBasisDataDownloadAvailable = false;
    }

    /// <summary>
    /// Performs validation on the Batch
    /// </summary>
    /// <param name="validationErrors"> out's a list of possible errors.</param>
    /// <returns>If the Batch is valid</returns>
    public bool IsValid(out List<string> validationErrors)
    {
        var errorMessages = new List<string>();
        if (!_gridAreaCodes.Any())
            errorMessages.Add("Batch must contain at least one grid area code.");

        if (PeriodStart >= PeriodEnd)
        {
            errorMessages.Add("periodStart is greater or equal to periodEnd");
        }

        // Validate that period end is set to 1 millisecond before midnight
        // The hard coded time zone will be refactored out of this class in an upcoming PR
        var dateTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!;
        var zonedDateTime = new ZonedDateTime(PeriodEnd.Plus(Duration.FromMilliseconds(1)), dateTimeZone);
        var localDateTime = zonedDateTime.LocalDateTime;
        if (localDateTime.TimeOfDay != LocalTime.Midnight)
        {
            errorMessages.Add(
                $"The period end '{PeriodEnd.ToString()}' must be one millisecond before midnight.");
        }

        validationErrors = errorMessages;
        return !errorMessages.Any();
    }

    /// <summary>
    /// Required by Entity Framework
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private Batch()
    {
        Id = Guid.NewGuid();
        _gridAreaCodes = new List<GridAreaCode>();
        _clock = SystemClock.Instance;
    }

    // Private setter is used implicitly by tests
    public Guid Id { get; private set; }

    public ProcessType ProcessType { get; }

    public IReadOnlyCollection<GridAreaCode> GridAreaCodes => _gridAreaCodes;

    public BatchExecutionState ExecutionState { get; private set; }

    public Instant? ExecutionTimeStart { get; private set; }

    public Instant? ExecutionTimeEnd { get; private set; }

    public JobRunId? RunId { get; private set; }

    /// <summary>
    /// Must be exactly at the beginning (at 00:00:00 o'clock) of the local date.
    /// </summary>
    public Instant PeriodStart { get; }

    /// <summary>
    /// Must be exactly 1 ms before the end (midnight) of the local date.
    /// The 1 ms off is by design originating from the front-end decision on how to handle date periods.
    /// </summary>
    public Instant PeriodEnd { get; }

    /// <summary>
    /// Gets an open-ended period end. That is a period end, which is exactly at midnight and thus exclusive.
    /// This is used in calculations as it prevents loss of e.g. time-series received in the last millisecond
    /// before midnight.
    /// </summary>
    public Instant OpenPeriodEnd => PeriodEnd.Plus(Duration.FromMilliseconds(1));

    public bool IsBasisDataDownloadAvailable { get; set; }

    public void MarkAsSubmitted(JobRunId jobRunId)
    {
        ArgumentNullException.ThrowIfNull(jobRunId);
        if (ExecutionState is BatchExecutionState.Submitted or BatchExecutionState.Pending
            or BatchExecutionState.Executing or BatchExecutionState.Completed or BatchExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, BatchExecutionState.Submitted);
        RunId = jobRunId;
        ExecutionState = BatchExecutionState.Submitted;
    }

    public void MarkAsPending()
    {
        if (ExecutionState is BatchExecutionState.Pending or BatchExecutionState.Executing or BatchExecutionState.Completed or BatchExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, BatchExecutionState.Pending);
        ExecutionState = BatchExecutionState.Pending;
    }

    public void MarkAsExecuting()
    {
        if (ExecutionState is BatchExecutionState.Executing or BatchExecutionState.Completed or BatchExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, BatchExecutionState.Executing);

        ExecutionState = BatchExecutionState.Executing;
    }

    public void MarkAsCompleted()
    {
        if (ExecutionState is BatchExecutionState.Completed or BatchExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, BatchExecutionState.Completed);

        ExecutionState = BatchExecutionState.Completed;
        ExecutionTimeEnd = _clock.GetCurrentInstant();
    }

    public void MarkAsFailed()
    {
        if (ExecutionState is BatchExecutionState.Failed)
            ThrowInvalidStateTransitionException(ExecutionState, BatchExecutionState.Failed);

        ExecutionState = BatchExecutionState.Failed;
    }

    /// <summary>
    /// Reset a batch. This will ensure that it will be picked up and run again in a new job run.
    /// </summary>
    public void Reset()
    {
        if (ExecutionState is BatchExecutionState.Completed)
            ThrowInvalidStateTransitionException(ExecutionState, BatchExecutionState.Created);

        ExecutionState = BatchExecutionState.Created;
    }

    private void ThrowInvalidStateTransitionException(BatchExecutionState currentState, BatchExecutionState desiredState)
    {
        throw new InvalidOperationException($"Cannot change batchExecutionState from {currentState} to {desiredState}");
    }
}
