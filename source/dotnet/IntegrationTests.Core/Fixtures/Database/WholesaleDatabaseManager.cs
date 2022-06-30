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

using Energinet.DataHub.Core.FunctionApp.TestCommon.Database;
using Energinet.DataHub.Wholesale.DatabaseMigration;
using Energinet.DataHub.Wholesale.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Wholesale.IntegrationTests.Core.Fixtures.Database;

public class WholesaleDatabaseManager : SqlServerDatabaseManager<DatabaseContext>
{
    public WholesaleDatabaseManager()
        : base("Wholesale")
    {
    }

    /// <inheritdoc/>
    public override DatabaseContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlServer(ConnectionString, options => options.UseNodaTime());

        return new DatabaseContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Creates the database schema using DbUp instead of a database context.
    /// </summary>
    protected override Task<bool> CreateDatabaseSchemaAsync(DatabaseContext context)
    {
        return Task.FromResult(CreateDatabaseSchema(context));
    }

    /// <summary>
    /// Creates the database schema using DbUp instead of a database context.
    /// </summary>
    protected override bool CreateDatabaseSchema(DatabaseContext context)
    {
        var result = Upgrader.DatabaseUpgrade(ConnectionString);
        if (!result.Successful)
            throw new Exception("Database migration failed", result.Error);

        return true;
    }
}
