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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.CalculationResults.UnitTests.Infrastructure.SqlStatements;

[UnitTest]
public class DatabricksSqlResponseParserTests
{
    private readonly string _succeededResultJson;
    private readonly string _resultChunkJson;
    private readonly string _pendingResultJson;
    private readonly string _runningResultJson;
    private readonly string _closedResultJson;
    private readonly string _canceledResultJson;
    private readonly string _failedResultJson;

    public DatabricksSqlResponseParserTests()
    {
        var stream = EmbeddedResources.GetStream("Infrastructure.SqlStatements.CalculationResult.json");
        using var reader = new StreamReader(stream);
        _succeededResultJson = reader.ReadToEnd();

        var chunkStream = EmbeddedResources.GetStream("Infrastructure.SqlStatements.CalculationResultChunk.json");
        using var chunkReader = new StreamReader(chunkStream);
        _resultChunkJson = chunkReader.ReadToEnd();

        _pendingResultJson = CreateResultJson("PENDING");
        _runningResultJson = CreateResultJson("RUNNING");
        _closedResultJson = CreateResultJson("CLOSED");
        _canceledResultJson = CreateResultJson("CANCELED");
        _failedResultJson = CreateResultJson("FAILED");
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenStateIsPending_ReturnsResponseWithExpectedState(DatabricksSqlResponseParser sut)
    {
        // Arrange
        const DatabricksSqlResponseState expectedState = DatabricksSqlResponseState.Pending;

        // Act
        var actual = sut.Parse(_pendingResultJson);

        // Assert
        actual.State.Should().Be(expectedState);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Parse_WhenStateIsSucceeded_ReturnsResponseWithExpectedState(
        DatabricksSqlChunkResponse chunkResponse,
        [Frozen] Mock<IDatabricksSqlChunkResponseParser> chunkParserMock,
        DatabricksSqlResponseParser sut)
    {
        // Arrange
        const DatabricksSqlResponseState expectedState = DatabricksSqlResponseState.Succeeded;
        chunkParserMock.Setup(x => x.Parse(It.IsAny<JToken>())).Returns(chunkResponse);

        // Act
        var actual = sut.Parse(_succeededResultJson);

        // Assert
        actual.State.Should().Be(expectedState);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenStateIsCanceled_ReturnsResponseWithExpectedState(DatabricksSqlResponseParser sut)
    {
        // Arrange
        const DatabricksSqlResponseState expectedState = DatabricksSqlResponseState.Cancelled;

        // Act
        var actual = sut.Parse(_canceledResultJson);

        // Assert
        actual.State.Should().Be(expectedState);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenStateIsRunning_ReturnsResponseWithExpectedState(DatabricksSqlResponseParser sut)
    {
        // Arrange
        const DatabricksSqlResponseState expectedState = DatabricksSqlResponseState.Running;

        // Act
        var actual = sut.Parse(_runningResultJson);

        // Assert
        actual.State.Should().Be(expectedState);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenStateIsClosed_ReturnsResponseWithExpectedState(DatabricksSqlResponseParser sut)
    {
        // Arrange
        const DatabricksSqlResponseState expectedState = DatabricksSqlResponseState.Closed;

        // Act
        var actual = sut.Parse(_closedResultJson);

        // Assert
        actual.State.Should().Be(expectedState);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenStateIsUnknown_LogsErrorAndThrowsDatabricksSqlException(
        [Frozen] Mock<ILogger<DatabricksSqlResponseParser>> loggerMock,
        DatabricksSqlResponseParser sut)
    {
        // Arrange
        var resultJson = CreateResultJson("UNKNOWN");

        // Act and assert
        Assert.Throws<DatabricksSqlException>(() => sut.Parse(resultJson));

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenStateIsFailed_ReturnsResponseWithExpectedState(DatabricksSqlResponseParser sut)
    {
        // Arrange
        const DatabricksSqlResponseState expectedState = DatabricksSqlResponseState.Failed;

        // Act
        var actual = sut.Parse(_failedResultJson);

        // Assert
        actual.State.Should().Be(expectedState);
    }

    // [Theory]
    // [AutoMoqData]
    // public void Parse_ReturnsResponseWithExpectedArrayLength(DatabricksSqlResponseParser sut)
    // {
    //     // Arrange
    //     const int expectedLength = 96;
    //
    //     // Act
    //     var actual = sut.Parse(_succeededResultJson);
    //
    //     // Assert
    //     actual.Table!.RowCount.Should().Be(expectedLength);
    // }
    //
    // [Theory]
    // [AutoMoqData]
    // public void Parse_ReturnsDataArrayWithExpectedContent(DatabricksSqlResponseParser sut)
    // {
    //     // Arrange
    //     var expectedFirstArray = new[]
    //     {
    //         "543", null, null, "0.000", "missing", "2023-04-04T22:00:00.000Z", "total_ga", "net_exchange_per_ga", "0ff76fd9-7d07-48f0-9752-e94d38d93498", "BalanceFixing", "2023-04-05T08:47:41.000Z", null,
    //     };
    //     var expectedLastArray = new[]
    //     {
    //         "543", null, null, "1.235", "estimated", "2023-04-05T21:45:00.000Z", "total_ga", "net_exchange_per_ga", "0ff76fd9-7d07-48f0-9752-e94d38d93498", "BalanceFixing", "2023-04-05T08:47:41.000Z", null,
    //     };
    //
    //     // Act
    //     var actual = sut.Parse(_succeededResultJson);
    //
    //     // Assert
    //     actual.Table![0].Should().Equal(expectedFirstArray);
    //     actual.Table![^1].Should().Equal(expectedLastArray);
    // }
    [Theory]
    [AutoMoqData]
    public void Parse_WhenValidJson_ReturnsResult(DatabricksSqlResponseParser sut)
    {
        // Arrange
        var statementId = new JProperty("statement_id", Guid.NewGuid());
        var status = new JProperty("status", new JObject(new JProperty("state", "PENDING")));
        var manifest = new JProperty("manifest", new JObject(new JProperty("schema", new JObject(new JProperty("columns", new JArray(new JObject(new JProperty("name", "grid_area"))))))));
        var result = new JProperty("result", new JObject(new JProperty("data_array", new List<string[]>())));
        var obj = new JObject(statementId, status, manifest, result);
        var jsonString = obj.ToString();

        // Act + Assert
        sut.Parse(jsonString).Should().NotBeNull();
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenInvalidJson_ThrowsException(DatabricksSqlResponseParser sut)
    {
        // Arrange
        var statementId = new JProperty("statement_id", Guid.NewGuid());
        var status = new JProperty("not_status", new JObject(new JProperty("state", "PENDING")));
        var manifest = new JProperty("manifest", new JObject(new JProperty("schema", new JObject(new JProperty("columns", new JArray(new JObject(new JProperty("name", "grid_area"))))))));
        var result = new JProperty("result", new JObject(new JProperty("data_array", new List<string[]>())));
        var obj = new JObject(statementId, status, manifest, result);
        var jsonString = obj.ToString();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => sut.Parse(jsonString));
    }

//     [Theory]
//     [AutoMoqData]
//     public void Parse_WhenNoDataMatchesCriteria_ReturnTableWithZeroRows(DatabricksSqlResponseParser sut)
//     {
//         // Arrange
//         var jsonString = @"{
//     'statement_id': '01edef23-0d2c-10dd-879b-26b5e97b3796',
//     'status': {
//         'state': 'SUCCEEDED'
//     },
//     'manifest': {
//         'schema': {
//             'columns': [
//                 {
//                     'name': 'grid_area'
//                 }
//             ]
//         },
//         'total_row_count': 0
//     },
//     'result': {
//         'row_count': 0
//     }
// }";
//
//         // Act
//         var actual = sut.Parse(jsonString);
//
//         // Assert
//         actual.Table!.RowCount.Should().Be(0);
//     }
//
//     [Theory(Skip = "...")]
//     [InlineAutoMoqData]
//     public void Parse_WhenResultIsChunk_ReturnsExpectedNextChunkInternalLink(DatabricksSqlResponseParser sut)
//     {
//         // Arrange
//         var expectedNextChunkInternalLink =
//             "/api/2.0/sql/statements/01ed92c5-3583-1f38-b21b-c6773e7c56b3/result/chunks/1?row_offset=432500";
//
//         // Act
//         var actual = sut.Parse(_resultChunkJson);
//
//         // Assert
//         actual.HasMoreRows.Should().BeTrue();
//         actual.ExternalLink.Should().Be(expectedNextChunkInternalLink);
//     }
    private string CreateResultJson(string state)
    {
        var statement = new
        {
            statement_id = "01edef23-0d2c-10dd-879b-26b5e97b3796",
            status = new { state, },
        };
        return JsonConvert.SerializeObject(statement, Formatting.Indented);
    }
}
