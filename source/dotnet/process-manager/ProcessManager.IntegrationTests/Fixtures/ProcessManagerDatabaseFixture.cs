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

using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestCommon.Fixture.Database;
using Xunit;

namespace Energinet.DataHub.Wholesale.ProcessManager.IntegrationTests.Fixtures;

public sealed class ProcessManagerDatabaseFixture : IAsyncLifetime
{
    public ProcessManagerDatabaseFixture()
    {
        DatabaseManager = new WholesaleDatabaseManager();
    }

    public WholesaleDatabaseManager DatabaseManager { get; }

    public async Task InitializeAsync()
    {
        await DatabaseManager.CreateDatabaseAsync();
        Environment.SetEnvironmentVariable(EnvironmentSettingNames.DatabaseConnectionString, DatabaseManager.ConnectionString);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
