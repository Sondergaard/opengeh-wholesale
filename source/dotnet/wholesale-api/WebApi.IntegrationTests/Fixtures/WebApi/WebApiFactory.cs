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

using Energinet.DataHub.Wholesale.Application.Batches;
using Energinet.DataHub.Wholesale.Application.ProcessStep;
using Energinet.DataHub.Wholesale.Application.SettlementReport;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.ProcessStep;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.WebApi;

/// <summary>
/// When we execute the tests on build agents we use the builded output (assemblies).
/// To avoid an 'System.IO.DirectoryNotFoundException' exception from WebApplicationFactory
/// during creation, we must set the path to the 'content root' using an environment variable
/// named 'ASPNETCORE_TEST_CONTENTROOT_ENERGINET_DATAHUB_WHOLESALE_WEBAPI'.
/// </summary>
public class WebApiFactory : WebApplicationFactory<Startup>
{
    private bool _authenticationEnabled;

    /// <summary>
    /// Integration tests run without authentication, unless explicitly enabled.
    /// </summary>
    public void ReenableAuthentication()
    {
        _authenticationEnabled = true;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        // This can be used for changing registrations in the container (e.g. for mocks).
        builder.ConfigureServices(services =>
        {
            if (!_authenticationEnabled)
            {
                services.AddSingleton<IAuthorizationHandler>(new AllowAnonymous());
            }

            services.AddScoped(_ =>
                SettlementReportApplicationServiceMock?.Object ??
                new Mock<ISettlementReportApplicationService>().Object);
            services.AddScoped(_ =>
                ProcessStepApplicationServiceMock?.Object ?? new Mock<IProcessStepApplicationService>().Object);
            services.AddScoped(_ => BatchApplicationServiceMock?.Object ?? new Mock<IBatchApplicationService>().Object);
        });
    }

    /// <summary>
    /// Allow configuring the behaviour of the <see cref="ISettlementReportApplicationService"/> by providing a custom <see cref="Moq.Mock{ISettlementReportApplicationService}" /> mock.
    /// NOTE: This will only work as expected as long as no tests are executed in parallel.
    /// </summary>
    public Mock<ISettlementReportApplicationService>? SettlementReportApplicationServiceMock { get; set; }

    public Mock<IProcessStepApplicationService>? ProcessStepApplicationServiceMock { get; set; }

    public Mock<IBatchApplicationService>? BatchApplicationServiceMock { get; set; }

    private sealed class AllowAnonymous : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var requirement in context.PendingRequirements.ToList())
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
