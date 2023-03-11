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

using Energinet.DataHub.Wholesale.Application.ProcessStep;
using Energinet.DataHub.Wholesale.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.V3.ProcessStepBalanceResponsibleParty;

/// <summary>
/// Energy suppliers for which batch results have been calculated.
/// </summary>
[Route("/v3/batches/{batchId}/processes/{gridAreaCode}/time-series-types/{timeSeriesType}/balance-responsible-parties")]
public class ProcessStepBalanceResponsiblePartyController : V3ControllerBase
{
    private readonly IProcessStepApplicationService _processStepApplicationService;

    public ProcessStepBalanceResponsiblePartyController(IProcessStepApplicationService processStepApplicationService)
    {
        _processStepApplicationService = processStepApplicationService;
    }

    /// <summary>
    /// Balance responsible parties.
    /// </summary>
    [AllowAnonymous] // TODO: Temporary hack to enable EDI integration while awaiting architects decision
    [HttpGet]
    [Produces("application/json", Type = typeof(List<ActorDto>))]
    public async Task<List<ActorDto>> GetAllAsync(
        [FromRoute] Guid batchId,
        [FromRoute] string gridAreaCode,
        [FromRoute] TimeSeriesType timeSeriesType)
    {
        var balanceResponsibleParties = await _processStepApplicationService.GetBalanceResponsiblePartiesAsync(batchId, gridAreaCode, timeSeriesType).ConfigureAwait(false);

        return balanceResponsibleParties
            .Select(a => new ActorDto(a.Gln))
            .ToList();
    }
}
