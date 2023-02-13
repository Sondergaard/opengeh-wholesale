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

using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessAggregate;
using Energinet.DataHub.Wholesale.ProcessManager.Endpoints;
using Energinet.DataHub.Wholesale.ProcessManager.IntegrationTests.Fixtures;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Function;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.ProcessManager.IntegrationTests;

public class SubmitCreatedBatchesEndpointTests
{
    [Collection(nameof(ProcessManagerFunctionAppCollectionFixture))]
    public class RunAsync : FunctionAppTestBase<ProcessManagerFunctionAppFixture>, IAsyncLifetime
    {
        public RunAsync(ProcessManagerFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture, testOutputHelper)
        {
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task When_PendingBatchCreated_Then_BatchIsCompleted()
        {
            await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(UpdateBatchExecutionStateEndpoint));
        }
    }
}
