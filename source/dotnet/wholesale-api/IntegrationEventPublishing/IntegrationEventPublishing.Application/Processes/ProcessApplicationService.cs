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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.ActorClient;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResultClient.Model;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.IntegrationEventsManagement;
using Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.Processes.Model;

namespace Energinet.DataHub.Wholesale.IntegrationEventPublishing.Application.Processes;

public class ProcessApplicationService : IProcessApplicationService
{
    private readonly IProcessStepResultRepository _processStepResultRepository;
    private readonly IActorClient _actorClient;
    private readonly ICalculationResultCompletedFactory _calculationResultCompletedFactory;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessApplicationService(
        IProcessStepResultRepository processStepResultRepository,
        IActorClient actorClient,
        ICalculationResultCompletedFactory integrationEventFactory,
        IIntegrationEventPublisher integrationEventPublisher,
        IUnitOfWork unitOfWork)
    {
        _processStepResultRepository = processStepResultRepository;
        _actorClient = actorClient;
        _calculationResultCompletedFactory = integrationEventFactory;
        _integrationEventPublisher = integrationEventPublisher;
        _unitOfWork = unitOfWork;
    }

    public async Task PublishCalculationResultCompletedIntegrationEventsAsync(ProcessCompletedEventDto processCompletedEvent)
    {
        // Publish events for energy suppliers
        await PublishCalculationResultCompletedForEnergySuppliersAsync(processCompletedEvent, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);

        // Publish events for total grid area - production
        await PublishCalculationResultCompletedForTotalGridAreaAsync(processCompletedEvent, TimeSeriesType.Production).ConfigureAwait(false);

        // Publish events for total grid area - non profiled
        await PublishCalculationResultCompletedForTotalGridAreaAsync(processCompletedEvent, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);

        // Publish events for balance responsible party
        await PublishCalculationResultCompletedForBalanceResponsiblePartiesAsync(processCompletedEvent, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);

        // Publish events for energy suppliers results for balance responsible parties
        await PublishCalculationResultCompletedForEnergySupplierBalanceResponsiblePartiesAsync(processCompletedEvent, TimeSeriesType.NonProfiledConsumption).ConfigureAwait(false);

        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    private async Task PublishCalculationResultCompletedForEnergySupplierBalanceResponsiblePartiesAsync(ProcessCompletedEventDto processCompletedEvent, TimeSeriesType timeSeriesType)
    {
        var brps = await _actorClient
            .GetBalanceResponsiblePartiesAsync(
                processCompletedEvent.BatchId,
                processCompletedEvent.GridAreaCode,
                timeSeriesType).ConfigureAwait(false);
        foreach (var brp in brps)
        {
            var energySuppliersByBalanceResponsibleParty = await _actorClient
                .GetEnergySuppliersByBalanceResponsiblePartyAsync(
                    processCompletedEvent.BatchId,
                    processCompletedEvent.GridAreaCode,
                    timeSeriesType,
                    brp.Gln).ConfigureAwait(false);

            foreach (var energySupplier in energySuppliersByBalanceResponsibleParty)
            {
                var result = await _processStepResultRepository.GetAsync(
                        processCompletedEvent.BatchId,
                        processCompletedEvent.GridAreaCode,
                        timeSeriesType,
                        energySupplier.Gln,
                        brp.Gln)
                    .ConfigureAwait(false);

                var integrationEvent = _calculationResultCompletedFactory.CreateForEnergySupplierByBalanceResponsibleParty(result, processCompletedEvent, energySupplier.Gln, brp.Gln);
                await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
            }
        }
    }

    private async Task PublishCalculationResultCompletedForTotalGridAreaAsync(ProcessCompletedEventDto processCompletedEvent, TimeSeriesType timeSeriesType)
    {
            var productionForTotalGa = await _processStepResultRepository
                .GetAsync(
                    processCompletedEvent.BatchId,
                    processCompletedEvent.GridAreaCode,
                    timeSeriesType,
                    null,
                    null)
                .ConfigureAwait(false);

            var integrationEvent = _calculationResultCompletedFactory.CreateForTotalGridArea(productionForTotalGa, processCompletedEvent);
            await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
    }

    private async Task PublishCalculationResultCompletedForEnergySuppliersAsync(ProcessCompletedEventDto processCompletedEvent, TimeSeriesType timeSeriesType)
    {
            var energySuppliers = await _actorClient.GetEnergySuppliersAsync(
                processCompletedEvent.BatchId,
                processCompletedEvent.GridAreaCode,
                timeSeriesType).ConfigureAwait(false);

            foreach (var energySupplier in energySuppliers)
            {
                var processStepResultDto = await _processStepResultRepository
                    .GetAsync(
                        processCompletedEvent.BatchId,
                        processCompletedEvent.GridAreaCode,
                        timeSeriesType,
                        energySupplier.Gln,
                        null)
                    .ConfigureAwait(false);

                var integrationEvent = _calculationResultCompletedFactory.CreateForEnergySupplier(processStepResultDto, processCompletedEvent, energySupplier.Gln);
                await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
            }
    }

    private async Task PublishCalculationResultCompletedForBalanceResponsiblePartiesAsync(ProcessCompletedEventDto processCompletedEvent, TimeSeriesType timeSeriesType)
    {
        var balanceResponsibleParties = await _actorClient.GetBalanceResponsiblePartiesAsync(
            processCompletedEvent.BatchId,
            processCompletedEvent.GridAreaCode,
            timeSeriesType).ConfigureAwait(false);

        foreach (var balanceResponsibleParty in balanceResponsibleParties)
        {
            var processStepResultDto = await _processStepResultRepository
                .GetAsync(
                    processCompletedEvent.BatchId,
                    processCompletedEvent.GridAreaCode,
                    timeSeriesType,
                    null,
                    balanceResponsibleParty.Gln)
                .ConfigureAwait(false);

            var integrationEvent = _calculationResultCompletedFactory.CreateForBalanceResponsibleParty(processStepResultDto, processCompletedEvent, balanceResponsibleParty.Gln);
            await _integrationEventPublisher.PublishAsync(integrationEvent).ConfigureAwait(false);
        }
    }
}
