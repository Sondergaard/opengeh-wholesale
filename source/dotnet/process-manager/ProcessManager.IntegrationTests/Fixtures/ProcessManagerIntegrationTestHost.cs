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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Wholesale.ProcessManager.IntegrationTests.Mock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Wholesale.ProcessManager.IntegrationTests.Fixtures;

public sealed class ProcessManagerIntegrationTestHost : IDisposable
{
    private readonly IHost _processManagerHost;

    private ProcessManagerIntegrationTestHost(IHost processManagerHost)
    {
        _processManagerHost = processManagerHost;
    }

    public static Task<ProcessManagerIntegrationTestHost> CreateAsync(
        string databaseManagerConnectionString,
        Action<IServiceCollection>? serviceConfiguration = default)
    {
        ConfigureEnvironmentVars(databaseManagerConnectionString);

        var hostBuilder = Program
            .CreateHostBuilder()
            .ConfigureServices(ConfigureServices);

        if (serviceConfiguration != null)
        {
            hostBuilder = hostBuilder.ConfigureServices(serviceConfiguration);
        }

        return Task.FromResult(new ProcessManagerIntegrationTestHost(hostBuilder.Build()));
    }

    public AsyncServiceScope BeginScope()
    {
        return _processManagerHost.Services.CreateAsyncScope();
    }

    public void Dispose()
    {
        _processManagerHost.Dispose();
    }

    private static void ConfigureEnvironmentVars(string databaseManagerConnectionString)
    {
        const string anyValue = "fake_value";
        const string anyServiceBusConnectionString = "Endpoint=sb://foo.servicebus.windows.net/;SharedAccessKeyName=someKeyName;SharedAccessKey=someKeyValue";

        Environment.SetEnvironmentVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusSendConnectionString, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ServiceBusManageConnectionString, anyServiceBusConnectionString);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventsTopicName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventsTopicName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.StartCalculationWhenBatchCreatedSubscriptionName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.PublishProcessesCompletedWhenCompletedBatchSubscriptionName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.CreateSettlementReportsWhenCompletedBatchSubscriptionName, anyValue);

        // Uses the well-known storage account name and key.
        // See: https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&bc=%2Fazure%2Fstorage%2Fblobs%2Fbreadcrumb%2Ftoc.json&tabs=visual-studio#well-known-storage-account-and-key
        var wellKnownStorageAccountName = "devstoreaccount1";
        var wellKnownStorageAccountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
        var blobStorageConnectionString = $"DefaultEndpointsProtocol=https;AccountName={wellKnownStorageAccountName};AccountKey={wellKnownStorageAccountKey};BlobEndpoint=https://localhost:10000/devstoreaccount1;";
        //// BlobEndpoint=https://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=https://127.0.0.1:10001/devstoreaccount1;TableEndpoint=https://127.0.0.1:10002/devstoreaccount1;

        Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageConnectionString, blobStorageConnectionString);
        ////Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageConnectionString, "UseDevelopmentStorage=true");

        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabaseConnectionString, databaseManagerConnectionString);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.CalculationStorageContainerName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.BatchCompletedEventName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.ProcessCompletedEventName, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabricksWorkspaceToken, anyValue);
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabricksWorkspaceUrl, "https://thisurlgoesnowhere.net/");
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DateTimeZoneId, "Europe/Copenhagen");
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.Replace(ServiceDescriptor.Singleton<ServiceBusClient, MockedServiceBusClient>());
        serviceCollection.Replace(ServiceDescriptor.Scoped<ICorrelationContext, MockedCorrelationContext>());
    }
}
