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

namespace Energinet.DataHub.Wholesale.Common.Databricks.Options;

public class DeltaTableOptions
{
    /// <summary>
    /// Name of the schema/database under which the result table is associated.
    /// </summary>
    public string SCHEMA_NAME { get; set; } = "wholesale_output";

    /// <summary>
    /// Name of the results delta table.
    /// </summary>
    public string RESULT_TABLE_NAME { get; set; } = "result";
}
