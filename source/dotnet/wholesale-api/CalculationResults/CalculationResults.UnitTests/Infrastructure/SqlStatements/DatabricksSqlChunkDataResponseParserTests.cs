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

using AutoFixture.Xunit2;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements
{
    public class DatabricksSqlChunkDataResponseParserTests
    {
        [Theory]
        [InlineAutoData]
        public void Parse_ReturnsExpectedTableChunk(DatabricksSqlChunkDataResponseParser sut)
        {
            // Arrange
            var jsonResponse = "[[\"John\", \"Doe\"], [\"Jane\", \"Smith\"]]";
            var expectedColumnNames = new[] { "FirstName", "LastName" };

            // Act
            var result = sut.Parse(jsonResponse, expectedColumnNames);

            // Assert
            result.ColumnNames.Should().BeEquivalentTo(expectedColumnNames);
            result.RowCount.Should().Be(2);
            result[0].Should().BeEquivalentTo("John", "Doe");
            result[1].Should().BeEquivalentTo("Jane", "Smith");
            result[0, "FirstName"].Should().Be("John");
            result[0, "LastName"].Should().Be("Doe");
        }

        [Theory]
        [InlineAutoData]
        public void Parse_WithInvalidJsonResponse_ThrowsInvalidOperationException(
            DatabricksSqlChunkDataResponseParser sut,
            string[] columnNames)
        {
            // Arrange
            var jsonResponse = "invalid json";

            // Act & Assert
            sut.Invoking(s => s.Parse(jsonResponse, columnNames))
                .Should().Throw<Exception>();
        }
    }
}
