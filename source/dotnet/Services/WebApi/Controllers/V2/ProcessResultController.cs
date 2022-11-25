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

using Energinet.DataHub.Wholesale.Application.ProcessResult;
using Energinet.DataHub.Wholesale.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.Controllers.V2;

/// <summary>
/// Handle process batches.
/// </summary>
[ApiController]
[ApiVersion(Version)]
[Route("v{version:apiVersion}/[controller]")]
public class ProcessResultController : ControllerBase
{
    private const string Version = "2.0";
    private readonly IProcessResultApplicationService _processResultApplicationService;

    public ProcessResultController(IProcessResultApplicationService processResultApplicationService)
    {
        _processResultApplicationService = processResultApplicationService;
    }

    [HttpGet]
    [MapToApiVersion(Version)]
    public async Task<IActionResult> GetAsync(Guid batchId, string gridAreaCode, ProcessStepType processStepType)
    {
        var resultDto = await _processResultApplicationService.GetResultAsync(batchId, gridAreaCode, processStepType).ConfigureAwait(false);
        return Ok(resultDto);
    }
}
