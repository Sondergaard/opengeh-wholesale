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

using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Wholesale.Application;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Contracts.Events;
using Energinet.DataHub.Wholesale.Domain.ProcessStepResultAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.Integration;
using Google.Protobuf;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.EventPublishers
{
    public class IntegrationEventFactory : IIntegrationEventFactory
    {
        private readonly IClock _systemDateTimeProvider;
        private readonly ICalculationResultCompletedIntegrationEventFactory _calculationResultCompletedIntegrationEventFactory;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IIntegrationEventTypeMapper _integrationEventTypeMapper;

        public IntegrationEventFactory(
            IClock systemDateTimeProvider,
            ICalculationResultCompletedIntegrationEventFactory calculationResultCompletedIntegrationEventFactory,
            IJsonSerializer jsonSerializer,
            IIntegrationEventTypeMapper integrationEventTypeMapper)
        {
            _systemDateTimeProvider = systemDateTimeProvider;
            _calculationResultCompletedIntegrationEventFactory = calculationResultCompletedIntegrationEventFactory;
            _jsonSerializer = jsonSerializer;
            _integrationEventTypeMapper = integrationEventTypeMapper;
        }

        public IntegrationEventDto CreateIntegrationEventForCalculationResultForEnergySupplier(ProcessStepResult processStepResult, ProcessCompletedEventDto processCompletedEventDto, string energySupplierGln)
        {
            var result = _calculationResultCompletedIntegrationEventFactory.CreateCalculationResultCompletedForEnergySupplier(processStepResult, processCompletedEventDto, energySupplierGln);
            return CreateIntegrationEvent(result);
        }

        public IntegrationEventDto CreateIntegrationEventForCalculationResultForBalanceResponsibleParty(ProcessStepResult processStepResultDto, ProcessCompletedEventDto processCompletedEventDto, string balanceResponsiblePartyGln)
        {
            var result = _calculationResultCompletedIntegrationEventFactory.CreateCalculationResultCompletedForBalanceResponsibleParty(processStepResultDto, processCompletedEventDto, balanceResponsiblePartyGln);
            return CreateIntegrationEvent(result);
        }

        public IntegrationEventDto CreateIntegrationEventForCalculationResultForTotalGridArea(ProcessStepResult processStepResult, ProcessCompletedEventDto processCompletedEventDto)
        {
            var result = _calculationResultCompletedIntegrationEventFactory.CreateCalculationResultCompletedForGridArea(processStepResult, processCompletedEventDto);
            return CreateIntegrationEvent(result);
        }

        public IntegrationEventDto CreateIntegrationEventForCalculationResultForEnergySupplierByBalanceResponsibleParty(ProcessStepResult processStepResultDto, ProcessCompletedEventDto processCompletedEvent, string energySupplierGln, string brpGln)
        {
            var result = _calculationResultCompletedIntegrationEventFactory.CreateCalculationResultForEnergySupplierByBalanceResponsibleParty(processStepResultDto, processCompletedEvent, energySupplierGln, brpGln);
            return CreateIntegrationEvent(result);
        }

        private IntegrationEventDto CreateIntegrationEvent(IMessage integrationEvent)
        {
            var eventMessageType = _integrationEventTypeMapper.GetMessageType(integrationEvent.GetType());
            var protobufByteArray = integrationEvent.ToByteArray();
            return new IntegrationEventDto(protobufByteArray, eventMessageType, _systemDateTimeProvider.GetCurrentInstant());
        }
    }
}
