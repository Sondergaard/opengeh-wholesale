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

namespace Energinet.DataHub.Wholesale.WebApi.V3.Batch;

public static class BatchDtoMapper
{
    public static BatchDto Map(Batches.Interfaces.Models.BatchDto batchDto)
    {
        if (batchDto == null) throw new ArgumentNullException(nameof(batchDto));

        return new BatchDto(
            batchDto.RunId,
            batchDto.BatchId,
            batchDto.PeriodStart,
            batchDto.PeriodEnd,
            batchDto.Resolution,
            batchDto.Unit,
            batchDto.ExecutionTimeStart,
            batchDto.ExecutionTimeEnd,
            BatchStateMapper.MapState(batchDto.ExecutionState),
            batchDto.AreSettlementReportsCreated,
            batchDto.GridAreaCodes,
            ProcessTypeMapper.Map(batchDto.ProcessType),
            batchDto.CreatedByUserId);
    }
}
