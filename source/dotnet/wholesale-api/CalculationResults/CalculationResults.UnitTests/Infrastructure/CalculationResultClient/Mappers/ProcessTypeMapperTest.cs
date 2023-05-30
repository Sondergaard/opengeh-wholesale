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

using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.Common.Models;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.CalculationResultClient.Mappers;

[UnitTest]
public class ProcessTypeMapperTests
{
    [Theory]
    [InlineData(ProcessType.Aggregation, DeltaTableProcessType.Aggregation)]
    [InlineData(ProcessType.BalanceFixing, DeltaTableProcessType.BalanceFixing)]
    public void ToDeltaTableValue_ReturnsExpectedString(ProcessType type, string expected)
    {
        // Act
        var actual = ProcessTypeMapper.ToDeltaTableValue(type);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(DeltaTableProcessType.Aggregation, ProcessType.Aggregation)]
    [InlineData(DeltaTableProcessType.BalanceFixing, ProcessType.BalanceFixing)]
    public void FromDeltaTableValue_ReturnsExpectedType(string deltaTableValue, ProcessType expected)
    {
        // Act
        var actual = ProcessTypeMapper.FromDeltaTableValue(deltaTableValue);

        // Assert
        actual.Should().Be(expected);
    }
}
