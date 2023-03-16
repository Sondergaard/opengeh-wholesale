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

using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.Hosts;
using Energinet.DataHub.Wholesale.WebApi.IntegrationTests.Fixtures.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.IntegrationTests;

[Collection(nameof(CompositionRootTests))]
public class CompositionRootTests
{
    #region Member data providers

    public static IEnumerable<object[]> GetControllerRequirements()
    {
        var constructorDependencies = ReflectionDelegates.FindAllConstructorDependenciesForType();

        return typeof(Program).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ControllerBase)))
            .Select(t => new object[] { new Requirement(t.Name, constructorDependencies(t)) });
    }

    #endregion

    [Theory]
    [MemberData(nameof(GetControllerRequirements))]
    public async Task WebApi_can_resolve_dependencies_for(Requirement requirement)
    {
        using var host = await WebApiIntegrationTestHost.CreateAsync();
        await using var scope = host.BeginScope();
        Assert.True(scope.ServiceProvider.CanSatisfyRequirement(requirement));
    }
}
