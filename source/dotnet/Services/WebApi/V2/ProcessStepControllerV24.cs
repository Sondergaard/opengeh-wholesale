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

namespace Energinet.DataHub.Wholesale.WebApi.V2;

[ApiController]
[Route("v2.4/ProcessStepResult")]
public class ProcessStepV24Controller : ControllerBase
{
    private readonly IProcessStepApplicationService _processStepApplicationService;

    public ProcessStepV24Controller(IProcessStepApplicationService processStepApplicationService)
    {
        _processStepApplicationService = processStepApplicationService;
    }

    /// <summary>
    /// Calculation results provided by the following method:
    /// When only 'EnergySupplierGln' is provided, a result is returned for a energy supplier for the requested grid area, for the specified time series type.
    /// if only a 'BalanceResponsiblePartyGln' is provided, a result is returned for a balance responsible party for the requested grid area, for the specified time series type.
    /// if both 'BalanceResponsiblePartyGln' and 'EnergySupplierGln' is provided, a result is returned for the balance responsible party's energy supplier for requested grid area, for the specified time series type.
    /// if no 'BalanceResponsiblePartyGln' and 'EnergySupplierGln' is provided, a result is returned for the requested grid area, for the specified time series type.
    /// </summary>
    [HttpPost]
    [ApiVersion("2.4")]
    [Produces("application/json", Type = typeof(ProcessStepResultDto))]
    public async Task<IActionResult> GetAsync([FromBody] ProcessStepResultRequestDtoV3 processStepResultRequestDtoV3)
    {
        var resultDto = await _processStepApplicationService.GetResultAsync(
            processStepResultRequestDtoV3.BatchId,
            processStepResultRequestDtoV3.GridAreaCode,
            processStepResultRequestDtoV3.TimeSeriesType,
            processStepResultRequestDtoV3.EnergySupplierGln,
            processStepResultRequestDtoV3.BalanceResponsiblePartyGln).ConfigureAwait(false);

        return Ok(resultDto);
    }
}
