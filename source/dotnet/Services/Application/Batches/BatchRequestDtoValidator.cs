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

using Energinet.DataHub.Wholesale.Contracts;
using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using NodaTime;
using NodaTime.Extensions;

namespace Energinet.DataHub.Wholesale.Application.Batches;

public class BatchRequestDtoValidator : IBatchRequestDtoValidator
{
    private readonly DateTimeZone _dateTimeZone;

    public BatchRequestDtoValidator(DateTimeZone dateTimeZone)
    {
        _dateTimeZone = dateTimeZone;
    }

    public bool IsValid(BatchRequestDto batchRequestDto, out IEnumerable<string> errorMessages)
    {
        var isValid = Batch.IsValid(
            batchRequestDto.GridAreaCodes.Select(code => new GridAreaCode(code)),
            batchRequestDto.StartDate.ToInstant(),
            batchRequestDto.EndDate.ToInstant(),
            _dateTimeZone,
            out var validationErrors);
        errorMessages = validationErrors;
        return isValid;
    }
}
