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

using System.Net;
using System.Net.Http.Headers;
using Energinet.DataHub.Wholesale.Common.Databricks.Options;
using Energinet.DataHub.Wholesale.Common.DatabricksClient;
using Microsoft.Azure.Databricks.Client;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Wholesale.Common.Databricks
{
    /// <summary>
    /// A databricks client based on the Microsoft.Azure.JobsApiClient, which is using Job API 2.0.
    /// The client is extended with a method for reading jobs created using Python Wheels, using Job API 2.1.
    /// Because the Job API 2.0 does not support reading python wheel settings.
    /// Which is used when we run new jobs and need to know the existing parameters of the job.
    /// The code is based on https://github.com/Azure/azure-databricks-client and can be replaced by the official
    /// package when support for Job API 2.1 is added.
    /// </summary>
    public sealed class JobsApiClient : IDisposable, IJobsApiClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Create client object with specified base URL, access token and timeout.
        /// </summary>
        /// <param name="optionsFactory">The databricks settings (options).</param>
        /// <param name="timeoutSeconds">Web request time out in seconds</param>
        public JobsApiClient(IOptions<DatabricksOptions> optionsFactory, long timeoutSeconds = 30)
        {
            var options = optionsFactory.Value;
            var apiUrl = new Uri(new Uri(options.DATABRICKS_WORKSPACE_URL), "api/");
            _httpClient = CreateHttpClient(options.DATABRICKS_WORKSPACE_TOKEN, timeoutSeconds, apiUrl);
            Jobs = new Microsoft.Azure.Databricks.Client.JobsApiClient(_httpClient);
        }

        private static HttpClient CreateHttpClient(string token, long timeoutSeconds, Uri apiUrl)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };

            var httpClient = new HttpClient(handler, false)
            {
                BaseAddress = apiUrl,
                Timeout = TimeSpan.FromSeconds(timeoutSeconds),
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            return httpClient;
        }

        public IJobsApi Jobs { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient.Dispose();
                Jobs.Dispose();
            }
        }
    }
}
