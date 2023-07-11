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

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Internal;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;
using Energinet.DataHub.Wholesale.Events.Application.UseCases;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Application.Communication;

public class IntegrationEventProvider : IIntegrationEventProvider
{
    private readonly ICalculationResultIntegrationEventFactory _calculationResultIntegrationEventFactory;
    private readonly ICalculationResultQueries _calculationResultQueries;
    private readonly ICompletedCalculationRepository _completedCalculationRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public IntegrationEventProvider(
        ICalculationResultIntegrationEventFactory integrationEventFactory,
        ICalculationResultQueries calculationResultQueries,
        ICompletedCalculationRepository completedCalculationRepository,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _calculationResultIntegrationEventFactory = integrationEventFactory;
        _calculationResultQueries = calculationResultQueries;
        _completedCalculationRepository = completedCalculationRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async IAsyncEnumerable<IntegrationEvent> GetAsync()
    {
        do
        {
            var completedCalculation = await _completedCalculationRepository.GetNextUnpublishedOrNullAsync().ConfigureAwait(false);
            if (completedCalculation == null) break;

            await foreach (var calculationResult in _calculationResultQueries.GetAsync(completedCalculation.Id).ConfigureAwait(false))
            {
                yield return _calculationResultIntegrationEventFactory.Create(calculationResult);
            }

            completedCalculation.PublishedTime = _clock.GetCurrentInstant();
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
        while (true);
    }
}
