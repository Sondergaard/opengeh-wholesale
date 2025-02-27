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

using Azure.Identity;
using Azure.Storage.Files.DataLake;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Wholesale.WebApi.HealthChecks
{
    public static class DataLakeHealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddDataLakeContainerCheck(this IHealthChecksBuilder builder, string storageAccountUri, string containerName)
        {
            return builder.AddAsyncCheck("DataLakeContainer", async () =>
            {
                try
                {
                    var serviceClient = new DataLakeServiceClient(new Uri(storageAccountUri), new DefaultAzureCredential());
                    var fileSystemClient = serviceClient.GetFileSystemClient(containerName);
                    return await fileSystemClient.ExistsAsync().ConfigureAwait(false)
                        ? HealthCheckResult.Healthy()
                        : HealthCheckResult.Unhealthy();
                }
                catch (Exception)
                {
                    return HealthCheckResult.Unhealthy();
                }
            });
        }
    }
}
