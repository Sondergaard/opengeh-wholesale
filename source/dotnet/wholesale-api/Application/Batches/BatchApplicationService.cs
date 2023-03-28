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

using Energinet.DataHub.Wholesale.Application.Batches.Model;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.BatchExecutionStateDomainService;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Application.Batches;

public class BatchApplicationService : IBatchApplicationService
{
    private readonly IBatchRepository _batchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBatchExecutionStateDomainService _batchExecutionStateDomainService;
    private readonly IBatchDtoMapper _batchDtoMapper;

    public BatchApplicationService(
        IBatchRepository batchRepository,
        IUnitOfWork unitOfWork,
        IBatchExecutionStateDomainService batchExecutionStateDomainService,
        IBatchDtoMapper batchDtoMapper)
    {
        _batchRepository = batchRepository;
        _unitOfWork = unitOfWork;
        _batchExecutionStateDomainService = batchExecutionStateDomainService;
        _batchDtoMapper = batchDtoMapper;
    }

    public async Task UpdateExecutionStateAsync()
    {
        await _batchExecutionStateDomainService.UpdateExecutionStateAsync().ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<BatchDto>> SearchAsync(
        IEnumerable<string> filterByGridAreaCodes,
        BatchState? filterByExecutionState,
        DateTimeOffset? minExecutionTime,
        DateTimeOffset? maxExecutionTime,
        DateTimeOffset? periodStart,
        DateTimeOffset? periodEnd)
    {
        var executionStateFilter = filterByExecutionState switch
        {
            null => Array.Empty<BatchExecutionState>(),
            BatchState.Pending => new[] { BatchExecutionState.Created, BatchExecutionState.Submitted, BatchExecutionState.Pending },
            BatchState.Executing => new[] { BatchExecutionState.Executing },
            BatchState.Completed => new[] { BatchExecutionState.Completed },
            BatchState.Failed => new[] { BatchExecutionState.Failed },
            _ => throw new ArgumentOutOfRangeException(nameof(filterByExecutionState)),
        };

        var gridAreaFilter = filterByGridAreaCodes
            .Select(g => new GridAreaCode(g))
            .ToList();

        var minExecutionTimeStart = ConvertToInstant(minExecutionTime);
        var maxExecutionTimeStart = ConvertToInstant(maxExecutionTime);
        var periodStartInstant = ConvertToInstant(periodStart);
        var periodEndInstant = ConvertToInstant(periodEnd);

        var batches = await _batchRepository
            .SearchAsync(
                gridAreaFilter,
                executionStateFilter,
                minExecutionTimeStart,
                maxExecutionTimeStart,
                periodStartInstant,
                periodEndInstant)
            .ConfigureAwait(false);

        return batches.Select(_batchDtoMapper.Map);
    }

    public async Task<BatchDto> GetAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId).ConfigureAwait(false);
        return _batchDtoMapper.Map(batch);
    }

    private static Instant? ConvertToInstant(DateTimeOffset? dateTimeOffset)
    {
        return dateTimeOffset == null
            ? null
            : Instant.FromDateTimeOffset(dateTimeOffset.Value);
    }
}
