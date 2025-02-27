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

using Energinet.DataHub.Core.Messaging.Communication.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Messaging.Communication;

public static class Registration
{
    /// <summary>
    /// Method for registering the communication library.
    /// It is the responsibility of the caller to register the dependencies of the
    /// <see cref="IIntegrationEventProvider"/> implementation.
    /// </summary>
    public static IServiceCollection AddCommunication<TIntegrationEventProvider>(
        this IServiceCollection services,
        string serviceBusIntegrationEventWriteConnectionString,
        string integrationEventTopicName)
        where TIntegrationEventProvider : class, IIntegrationEventProvider
    {
        services.AddHostedService<OutboxSenderTrigger>();
        services.AddSingleton<IHostedServiceReadinessMonitor, HostedServiceReadinessMonitor>();
        services.AddScoped<IIntegrationEventProvider, TIntegrationEventProvider>();
        services.AddSingleton<IServiceBusSenderProvider>(
            _ => new ServiceBusSenderProvider(serviceBusIntegrationEventWriteConnectionString, integrationEventTopicName));
        services.AddScoped<IOutboxSender, OutboxSender>();
        services.AddScoped<IServiceBusMessageFactory, ServiceBusMessageFactory>();

        return services;
    }
}
