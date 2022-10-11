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
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Contracts.WholesaleProcess;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.ProcessManager.Endpoints;

public class PublishProcessCompletedEvents
{
    private const string FunctionName = nameof(PublishProcessCompletedEvents);
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IProcessApplicationService _processApplicationService;

    public PublishProcessCompletedEvents(IJsonSerializer jsonSerializer, IProcessApplicationService processApplicationService)
    {
        _jsonSerializer = jsonSerializer;
        _processApplicationService = processApplicationService;
    }

    [Function(FunctionName)]
    public async Task RunAsync(
        [ServiceBusTrigger(
            "%" + EnvironmentSettingNames.BatchCompletedTopicName + "%",
            "%" + EnvironmentSettingNames.BatchCompletedSubscriptionName + "%",
            Connection = EnvironmentSettingNames.ServiceBusListenConnectionString)]
        byte[] message)
    {
        var batchCompletedEvent = await DeserializeByteArrayAsync<BatchCompletedEventDto>(message).ConfigureAwait(false);
        await _processApplicationService.PublishProcessCompletedEventsAsync(batchCompletedEvent).ConfigureAwait(false);
    }

    private async Task<T> DeserializeByteArrayAsync<T>(byte[] data)
    {
        var stream = new MemoryStream(data);
        await using (stream.ConfigureAwait(false))
        {
            return (T)await _jsonSerializer.DeserializeAsync(stream, typeof(T)).ConfigureAwait(false);
        }
    }
}
