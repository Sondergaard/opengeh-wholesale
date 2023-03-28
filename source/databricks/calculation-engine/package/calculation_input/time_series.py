# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from pyspark.sql import DataFrame
import pyspark.sql.functions as F
from package.constants import Colname
from package.codelists import (
    MigratedTimeSeriesQuality,
    TimeSeriesQuality,
)


def map_cim_quality_to_wholesale_quality(timeseries_point_df: DataFrame) -> DataFrame:
    "Map input CIM quality names to wholesale quality names"
    return timeseries_point_df.withColumn(
        Colname.quality,
        F.when(
            F.col(Colname.quality) == MigratedTimeSeriesQuality.missing.value,
            TimeSeriesQuality.missing.value,
        )
        .when(
            F.col(Colname.quality) == MigratedTimeSeriesQuality.estimated.value,
            TimeSeriesQuality.estimated.value,
        )
        .when(
            F.col(Colname.quality) == MigratedTimeSeriesQuality.measured.value,
            TimeSeriesQuality.measured.value,
        )
        .when(
            F.col(Colname.quality) == MigratedTimeSeriesQuality.calculated.value,
            TimeSeriesQuality.calculated.value,
        )
        .otherwise("UNKNOWN"),
    )
