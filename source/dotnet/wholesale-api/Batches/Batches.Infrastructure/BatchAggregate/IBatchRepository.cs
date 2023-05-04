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

using Energinet.DataHub.Wholesale.Batches.Infrastructure.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Batches.Infrastructure.BatchAggregate;

public interface IBatchRepository
{
    Task AddAsync(Batch batch);

    Task<Batch> GetAsync(Guid batchId);

    Task<List<Batch>> GetCreatedAsync();

    Task<List<Batch>> GetPendingAsync();

    Task<List<Batch>> GetExecutingAsync();

    Task<List<Batch>> GetByStatesAsync(IEnumerable<BatchExecutionState> states);

    Task<List<Batch>> GetCompletedAsync();

    Task<IReadOnlyCollection<Batch>> SearchAsync(
        IReadOnlyCollection<GridAreaCode> filterByGridAreaCode,
        IReadOnlyCollection<BatchExecutionState> filterByExecutionState,
        Instant? minExecutionTimeStart,
        Instant? maxExecutionTimeStart,
        Instant? periodStart,
        Instant? periodEnd);
}
