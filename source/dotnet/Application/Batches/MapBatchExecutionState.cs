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
// limitations under the License.using Energinet.DataHub.Wholesale.Application.JobRunner;

using Energinet.DataHub.Wholesale.Application.JobRunner;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;

namespace Energinet.DataHub.Wholesale.Application.Batches;

public class MapBatchExecutionState
{
    /// <summary>
    /// Update the execution states in the batch repository by mapping the job states from the runs <see cref="ICalculatorJobRunner"/>
    /// </summary>
    /// <returns>Batches that have been completed</returns>
    public async Task<IEnumerable<Batch>> UpdateExecutionStatesInBatchRepositoryAsync(IBatchRepository batchRepository, ICalculatorJobRunner calculatorJobRunner)
    {
        var completedBatches = new List<Batch>();
        var pendingAndExecutingBatches = await batchRepository.GetPendingAndExecutingAsync().ConfigureAwait(false);
        foreach (var batch in pendingAndExecutingBatches)
        {
            if (batch.RunId == null)
                continue;

            var jobState = await calculatorJobRunner
                .GetJobStateAsync(batch.RunId!)
                .ConfigureAwait(false);

            var executionState = MapState(jobState);
            if (executionState != batch.ExecutionState)
            {
                HandleNewState(executionState, batch, completedBatches);
            }
        }

        return completedBatches;
    }

    private static BatchExecutionState MapState(JobState jobState)
    {
        return jobState switch
        {
            JobState.Pending => BatchExecutionState.Pending,
            JobState.Canceled => BatchExecutionState.Completed,
            JobState.Running => BatchExecutionState.Executing,
            JobState.Completed => BatchExecutionState.Completed,
            JobState.Failed => BatchExecutionState.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(jobState), jobState, "Unexpected JobState."),
        };
    }

    private static void HandleNewState(BatchExecutionState state, Batch batch, ICollection<Batch> completedBatches)
    {
        switch (state)
        {
            case BatchExecutionState.Pending:
                break;
            case BatchExecutionState.Executing:
                batch.MarkAsExecuting();
                break;
            case BatchExecutionState.Completed:
                batch.MarkAsCompleted();
                completedBatches.Add(batch);
                break;
            case BatchExecutionState.Failed:
                batch.MarkAsFailed();
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unexpected execution state: {state.ToString()}.");
        }
    }
}
