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
using Energinet.DataHub.Wholesale.Application.Processes.Model;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.BatchExecutionStateDomainService;
using Energinet.DataHub.Wholesale.Domain.CalculationDomainService;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Application.Batches;

public class BatchApplicationService : IBatchApplicationService
{
    private readonly IBatchFactory _batchFactory;
    private readonly IBatchRepository _batchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalculationDomainService _calculationDomainService;
    private readonly ICalculationParametersFactory _calculationParametersFactory;
    private readonly IBatchExecutionStateDomainService _batchExecutionStateDomainService;
    private readonly IBatchDtoMapper _batchDtoMapper;
    private readonly IProcessTypeMapper _processTypeMapper;

    public BatchApplicationService(
        IBatchFactory batchFactory,
        IBatchRepository batchRepository,
        IUnitOfWork unitOfWork,
        ICalculationDomainService calculationDomainService,
        ICalculationParametersFactory calculationParametersFactory,
        IBatchExecutionStateDomainService batchExecutionStateDomainService,
        IBatchDtoMapper batchDtoMapper,
        IProcessTypeMapper processTypeMapper)
    {
        _batchFactory = batchFactory;
        _batchRepository = batchRepository;
        _unitOfWork = unitOfWork;
        _calculationDomainService = calculationDomainService;
        _calculationParametersFactory = calculationParametersFactory;
        _batchExecutionStateDomainService = batchExecutionStateDomainService;
        _batchDtoMapper = batchDtoMapper;
        _processTypeMapper = processTypeMapper;
    }

    public async Task<Guid> CreateAsync(BatchRequestDto batchRequestDto)
    {
        var processType = _processTypeMapper.MapFrom(batchRequestDto.ProcessType);
        var batch = _batchFactory.Create(processType, batchRequestDto.GridAreaCodes, batchRequestDto.StartDate, batchRequestDto.EndDate);
        await _batchRepository.AddAsync(batch).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);

        return batch.Id;
    }

    public async Task StartSubmittingAsync()
    {
        var batches = await _batchRepository.GetCreatedAsync().ConfigureAwait(false);

        // TODO: Problems with this code:
        // - Multiple unit of work commits. What if something fails? There should probably be exactly none or one commit per use case
        // - This complexity belongs to a domain service, but it can't be moved to a domain service because of the unit of work dependency
        // - ICalculationParametersFactory is an infrastructure concern, but can't be moved to infra due to this code
        foreach (var batch in batches)
        {
            var jobParameters = _calculationParametersFactory.CreateParameters(batch);
            var jobRunId = await _calculationDomainService.StartAsync(jobParameters).ConfigureAwait(false);
            batch.MarkAsSubmitted(jobRunId);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
    }

    public async Task UpdateExecutionStateAsync()
    {
        await _batchExecutionStateDomainService.UpdateExecutionStateAsync().ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<BatchDto>> SearchAsync(BatchSearchDto batchSearchDto)
    {
        var minExecutionTimeStart = Instant.FromDateTimeOffset(batchSearchDto.MinExecutionTime);
        var maxExecutionTimeStart = Instant.FromDateTimeOffset(batchSearchDto.MaxExecutionTime);

        var batches = await _batchRepository.GetAsync(minExecutionTimeStart, maxExecutionTimeStart)
            .ConfigureAwait(false);

        return batches.Select(_batchDtoMapper.Map);
    }

    public async Task<BatchDto> GetAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId).ConfigureAwait(false);
        return _batchDtoMapper.Map(batch);
    }
}
