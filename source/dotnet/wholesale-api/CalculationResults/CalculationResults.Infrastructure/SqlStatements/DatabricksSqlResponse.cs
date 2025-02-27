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

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;

public class DatabricksSqlResponse
{
    private DatabricksSqlResponse(Guid statementId, DatabricksSqlResponseState state, string[]? columnNames = null, DatabricksSqlChunkResponse? chunk = null)
    {
        StatementId = statementId;
        State = state;
        ColumnNames = columnNames;
        Chunk = chunk;
    }

    public static DatabricksSqlResponse CreateAsPending(Guid statementId)
    {
        return new DatabricksSqlResponse(statementId, DatabricksSqlResponseState.Pending);
    }

    public static DatabricksSqlResponse CreateAsRunning(Guid statementId)
    {
        return new DatabricksSqlResponse(statementId, DatabricksSqlResponseState.Running);
    }

    public static DatabricksSqlResponse CreateAsCancelled(Guid statementId)
    {
        return new DatabricksSqlResponse(statementId, DatabricksSqlResponseState.Cancelled);
    }

    public static DatabricksSqlResponse CreateAsSucceeded(Guid statementId, string[] columnNames, DatabricksSqlChunkResponse chunk)
    {
        return new DatabricksSqlResponse(statementId, DatabricksSqlResponseState.Succeeded, columnNames, chunk);
    }

    public static DatabricksSqlResponse CreateAsFailed(Guid statementId)
    {
        return new DatabricksSqlResponse(statementId, DatabricksSqlResponseState.Failed);
    }

    public static DatabricksSqlResponse CreateAsClosed(Guid statementId)
    {
        return new DatabricksSqlResponse(statementId, DatabricksSqlResponseState.Closed);
    }

    public Guid StatementId { get; }

    public DatabricksSqlResponseState State { get; }

    public DatabricksSqlChunkResponse? Chunk { get; }

    public string[]? ColumnNames { get; }
}
