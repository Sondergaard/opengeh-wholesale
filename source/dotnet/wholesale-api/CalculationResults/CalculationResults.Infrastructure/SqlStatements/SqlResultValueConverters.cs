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
using Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using NodaTime;
using NodaTime.Text;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.SqlStatements;

public static class SqlResultValueConverters
{
    public static Instant? ToInstant(string? value)
    {
        if (value == null) return null;
        return InstantPattern.ExtendedIso.Parse(value).Value;
    }

    public static decimal? ToDecimal(string? value)
    {
        if (value == null) return null;
        return decimal.Parse(value, CultureInfo.InvariantCulture);
    }

    public static DateTimeOffset? ToDateTimeOffset(string? value)
    {
        if (value == null) return null;
        return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
    }

    public static QuantityQuality ToQuantityQuality(string value)
    {
        return QuantityQualityMapper.FromDeltaTableValue(value);
    }

    public static TimeSeriesType ToTimeSeriesType(string value)
    {
        return TimeSeriesTypeMapper.FromDeltaTableValue(value);
    }

    public static Guid ToGuid(string value)
    {
        return Guid.Parse(value);
    }
}
