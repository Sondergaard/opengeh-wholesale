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

from package.constants import Colname
from pyspark.sql.types import (
    BooleanType,
    StructType,
    StructField,
    StringType,
    TimestampType,
)

grid_loss_sys_corr_schema = StructType(
    [
        StructField(Colname.metering_point_id, StringType(), False),
        StructField(Colname.grid_area, StringType(), False),
        StructField(Colname.energy_supplier_id, StringType(), False),
        StructField(Colname.is_positive_grid_loss_responsible, BooleanType(), False),
        StructField(Colname.is_negative_grid_loss_responsible, BooleanType(), False),
        StructField(Colname.from_date, TimestampType(), False),
        StructField(Colname.to_date, TimestampType(), False),
    ]
)
