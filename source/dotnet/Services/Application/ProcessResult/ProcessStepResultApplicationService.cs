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

using System.Globalization;
using System.Text.Json;
using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Domain.ProcessOutput;

namespace Energinet.DataHub.Wholesale.Application.ProcessResult;

/// <summary>
/// This class provides the ability to retrieve a calculated result for a given step for a batch.
/// </summary>
public class ProcessStepResultApplicationService : IProcessStepResultApplicationService
{
    private readonly IProcessOutputRepository _processOutputRepository;
    private readonly IProcessActorResultMapper _processActorResultMapper;

    public ProcessStepResultApplicationService(IProcessOutputRepository processOutputRepository, IProcessActorResultMapper processActorResultMapper)
    {
        _processOutputRepository = processOutputRepository;
        _processActorResultMapper = processActorResultMapper;
    }

    public async Task<ProcessStepResultDto> GetResultAsync(ProcessStepResultRequestDto processStepResultRequestDto)
    {
        var processActorResult = await _processOutputRepository.GetAsync(
                processStepResultRequestDto.BatchId,
                new GridAreaCode(processStepResultRequestDto.GridAreaCode))
            .ConfigureAwait(false);

        return _processActorResultMapper.MapToDto(processActorResult);
    }
}
