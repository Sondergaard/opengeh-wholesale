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

from geh_stream.codelists import Colname
from pyspark.sql.types import (
    DecimalType,
    IntegerType,
    StructType,
    StructField,
    StringType,
    TimestampType,
)


calculate_daily_subscription_price_schema = StructType(
    [
        StructField(Colname.charge_key, StringType(), False),
        StructField(Colname.charge_id, StringType(), False),
        StructField(Colname.charge_type, StringType(), False),
        StructField(Colname.charge_owner, StringType(), False),
        StructField(Colname.charge_price, DecimalType(18, 8), False),
        StructField(Colname.time, TimestampType(), False),
        StructField(Colname.price_per_day, DecimalType(18, 8), False),
        StructField(Colname.charge_count, IntegerType(), False),
        StructField(Colname.total_daily_charge_price, DecimalType(18, 8), False),
        StructField(Colname.metering_point_type, StringType(), False),
        StructField(Colname.settlement_method, StringType(), False),
        StructField(Colname.grid_area, StringType(), False),
        StructField(Colname.connection_state, StringType(), False),
        StructField(Colname.energy_supplier_id, StringType(), False),
    ]
)
