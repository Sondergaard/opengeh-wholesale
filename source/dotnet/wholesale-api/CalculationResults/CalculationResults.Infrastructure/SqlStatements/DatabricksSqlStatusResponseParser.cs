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

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;

public class DatabricksSqlStatusResponseParser : IDatabricksSqlStatusResponseParser
{
    private readonly ILogger<DatabricksSqlStatusResponseParser> _logger;
    private readonly IDatabricksSqlChunkResponseParser _chunkParser;

    public DatabricksSqlStatusResponseParser(ILogger<DatabricksSqlStatusResponseParser> logger, IDatabricksSqlChunkResponseParser chunkParser)
    {
        _logger = logger;
        _chunkParser = chunkParser;
    }

    public DatabricksSqlResponse Parse(string jsonResponse)
    {
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, };
        var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonResponse, settings) ??
                         throw new InvalidOperationException();
        var statementId = GetStatementId(jsonObject);
        var state = GetState(jsonObject);
        switch (state)
        {
            case "PENDING":
                return DatabricksSqlResponse.CreateAsPending(statementId);
            case "RUNNING":
                return DatabricksSqlResponse.CreateAsRunning(statementId);
            case "CLOSED":
                return DatabricksSqlResponse.CreateAsClosed(statementId);
            case "CANCELED":
                return DatabricksSqlResponse.CreateAsCancelled(statementId);
            case "FAILED":
                return DatabricksSqlResponse.CreateAsFailed(statementId);
            case "SUCCEEDED":
                var columnNames = GetColumnNames(jsonObject);
                var chunk = _chunkParser.Parse(GetChunk(jsonObject));
                return DatabricksSqlResponse.CreateAsSucceeded(statementId, columnNames, chunk);
            default:
                _logger.LogError("Databricks SQL statement execution failed. Response {JsonResponse}", jsonResponse);
                throw new DatabricksSqlException($@"Databricks SQL statement execution failed. State: {state}");
        }
    }

    private static Guid GetStatementId(JObject responseJsonObject)
    {
        return responseJsonObject["statement_id"]!.ToObject<Guid>();
    }

    private static string GetState(JObject responseJsonObject)
    {
        return responseJsonObject["status"]?["state"]?.ToString() ?? throw new InvalidOperationException("Unable to retrieve 'state' from the responseJsonObject");
    }

    private static string[] GetColumnNames(JObject responseJsonObject)
    {
        var columnNames = responseJsonObject["manifest"]?["schema"]?["columns"]?.Select(x => x["name"]?.ToString()).ToArray() ??
                          throw new DatabricksSqlException("Unable to retrieve 'columns' from the responseJsonObject.");
        return columnNames!;
    }

    private static JToken GetChunk(JObject responseJsonObject)
    {
        return responseJsonObject["result"]!;
    }
}
