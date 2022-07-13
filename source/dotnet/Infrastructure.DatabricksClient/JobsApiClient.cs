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

using Microsoft.Azure.Databricks.Client;

namespace Energinet.DataHub.Wholesale.Infrastructure.DatabricksClient
{
    public class JobsApiClient21 : JobsApiClient, IJobsApi21
    {
        public JobsApiClient21(HttpClient httpClient)
            : base(httpClient)
        {
        }

        public async Task<IEnumerable<Job21>> List21(CancellationToken cancellationToken = default)
        {
            const string requestUri = "jobs/list";
            var jobList = await HttpGet<dynamic>(HttpClient, requestUri, cancellationToken).ConfigureAwait(false);
            return PropertyExists(jobList, "jobs")
                ? jobList.jobs.ToObject<IEnumerable<Job21>>()
                : Enumerable.Empty<Job21>();
        }
    }
}
