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

using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Wholesale.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Application.Workers;

/// <summary>
/// Timer triggered hosted service to invoke the service for integration events retention.
/// </summary>
public class FetchBatchesReadyForExecutionWorker : BackgroundService
{
    // Execute once per day (86400 seconds)
    private const int DelayInSecondsBeforeNextExecution = 10;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FetchBatchesReadyForExecutionWorker> _logger;

    public FetchBatchesReadyForExecutionWorker(IServiceProvider serviceProvider, ILogger<FetchBatchesReadyForExecutionWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("{Worker} running at: {Time}", nameof(FetchBatchesReadyForExecutionWorker), DateTimeOffset.Now);

            await ExecuteInScopeAsync().ConfigureAwait(false);

            await Task.Delay(DelayInSecondsBeforeNextExecution * 1000, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task ExecuteInScopeAsync()
    {
        using var scope = _serviceProvider.CreateScope();

        // CorrelationIdMiddleware does not support IHostedService, so we need to add a correlation ID ourselves
        var correlationContext = scope.ServiceProvider.GetRequiredService<ICorrelationContext>();
        correlationContext.SetId(Guid.NewGuid().ToString());

        var handler = scope.ServiceProvider.GetRequiredService<IRegisterCompletedBatchesHandler>();
        await handler.RegisterCompletedBatchesAsync().ConfigureAwait(false);
    }
}
