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

using Energinet.DataHub.Wholesale.Components.DatabricksClient;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.CalculationDomainService;
using Microsoft.Azure.Databricks.Client.Models;

namespace Energinet.DataHub.Wholesale.Infrastructure.Calculations;

public sealed class CalculationEngineClient : ICalculationEngineClient
{
    private readonly IDatabricksCalculatorJobSelector _databricksCalculatorJobSelector;
    private readonly IDatabricksWheelClient _wheelClient;
    private readonly ICalculationParametersFactory _calculationParametersFactory;

    public CalculationEngineClient(
        IDatabricksCalculatorJobSelector databricksCalculatorJobSelector,
        IDatabricksWheelClient wheelClient,
        ICalculationParametersFactory calculationParametersFactory)
    {
        _databricksCalculatorJobSelector = databricksCalculatorJobSelector;
        _wheelClient = wheelClient;
        _calculationParametersFactory = calculationParametersFactory;
    }

    public async Task<CalculationId> StartAsync(Batch batch)
    {
        var jobParameters = _calculationParametersFactory.CreateParameters(batch);

        var calculatorJob = await _databricksCalculatorJobSelector
            .GetAsync()
            .ConfigureAwait(false);

        var runParameters = MergeRunParameters(calculatorJob, jobParameters);

        var runId = await _wheelClient
            .Jobs
            .RunNow(calculatorJob.JobId, runParameters)
            .ConfigureAwait(false);

        return new CalculationId(runId);
    }

    public async Task<CalculationState> GetStatusAsync(CalculationId calculationId)
    {
        var runState = await _wheelClient
            .Jobs
            .RunsGet(calculationId.Id)
            .ConfigureAwait(false);

        return runState.Item1.State.LifeCycleState switch
        {
            RunLifeCycleState.PENDING => CalculationState.Pending,
            RunLifeCycleState.RUNNING => CalculationState.Running,
            RunLifeCycleState.TERMINATING => CalculationState.Running,
            RunLifeCycleState.SKIPPED => CalculationState.Canceled,
            RunLifeCycleState.INTERNAL_ERROR => CalculationState.Failed,
            RunLifeCycleState.TERMINATED => runState.Item1.State.ResultState switch
            {
                RunResultState.SUCCESS => CalculationState.Completed,
                RunResultState.FAILED => CalculationState.Failed,
                RunResultState.CANCELED => CalculationState.Canceled,
                RunResultState.TIMEDOUT => CalculationState.Canceled,
                _ => throw new ArgumentOutOfRangeException(nameof(runState.Item1.State)),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(runState.Item1.State)),
        };
    }

    private static RunParameters MergeRunParameters(WheelJob job, IEnumerable<string> jobParameters)
    {
        var pythonWheelTask = job.Settings.Tasks.FirstOrDefault(x => x.PythonWheelTask != null);

        var sourceParams =
            pythonWheelTask?.PythonWheelTask?.Parameters ??
            throw new InvalidOperationException($"Parameters for job {job.JobId} could not be found.");

        sourceParams.AddRange(jobParameters);

        return RunParameters.CreatePythonParams(sourceParams);
    }
}
