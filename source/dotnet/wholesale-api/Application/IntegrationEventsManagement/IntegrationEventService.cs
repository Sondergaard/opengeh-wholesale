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

using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Application.IntegrationEventsManagement;

public class IntegrationEventService : IIntegrationEventService
{
    private readonly IOptions<IntegrationEventRetentionOptions> _options;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventDispatcher _integrationEventDispatcher;
    private readonly IIntegrationEventCleanUpService _integrationEventCleanUpService;

    public IntegrationEventService(
        IOptions<IntegrationEventRetentionOptions> options,
        IUnitOfWork unitOfWork,
        IIntegrationEventDispatcher integrationEventDispatcher,
        IIntegrationEventCleanUpService integrationEventCleanUpService)
    {
        _options = options;
        _unitOfWork = unitOfWork;
        _integrationEventDispatcher = integrationEventDispatcher;
        _integrationEventCleanUpService = integrationEventCleanUpService;
    }

    public async Task DispatchIntegrationEventsAsync()
    {
        const int numberOfIntegrationEvents = 1000;
        var moreToDispatch = true;
        while (moreToDispatch)
        {
            moreToDispatch = await _integrationEventDispatcher.DispatchIntegrationEventsAsync(numberOfIntegrationEvents).ConfigureAwait(false);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
    }

    public async Task DeleteOlderDispatchedIntegrationEventsAsync()
    {
        _integrationEventCleanUpService.DeleteOlderDispatchedIntegrationEvents(_options.Value.RetentionDays);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }
}
