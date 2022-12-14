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

from pyspark.sql.types import (
    BooleanType,
    IntegerType,
    StructField,
    StringType,
    TimestampType,
    StructType,
)

"""
Schema for charge link periods (and charges)

Charge link periods are only used in settlement.
Periods (given by `FromDate` and `ToDate`) must not overlap but may have gaps. Gaps may occur if the link has been removed for a period before being added again.

Data must be stored in a Delta table.

The table must be partitioned by `ToDate`: ToDate_Year/ToDate_Month/ToDate_Day.
It is important to partition by to-date instead of from-date as it will ensure efficient data filtering.
This is because most periods will have a to-date prior to the calculation period start date.

The table data must always contain updated periods.
"""
charge_link_period_schema = StructType(
    [
        # ID of the charge
        # The ID is only guaranteed to be unique for a specific actor and charge type.
        # The ID is provided by the charge owner (actor).
        # Example: 0010643756
        StructField("ChargeId", StringType(), False),

        # "D01" (subscription) | "D02 (fee) | "D03" (tariff)
        # Example: D01
        StructField("ChargeType", StringType(), False),

        # The unique GLN/EIC number of the charge owner (actor)
        # Example: 8100000000030
        StructField("ChargeOwnerId", StringType(), False),

        # "PT1H" (hourly) | "P1D" (daily) | "P1M" (monthly)
        # Behaviour depends on the type of the charge.
        # - Subscriptions: Always monthly
        # - Fees: Always monthly. The value is charged on the effective day on the metering point
        # - Tariffs: Only hourly and daily resolution applies
        # Example: PT1H
        StructField("Resolution", StringType(), False),

        # Specifies whether the charge is tax. Applies only to tariffs.
        # For subscriptions and fees the value must be false.
        # Example: True
        StructField("IsTax", BooleanType(), False),

        # GSRN (18 characters) that uniquely identifies the metering point
        # The field is from the charge link.
        # Example: 578710000000000103
        StructField("MeteringPointId", StringType(), False),
        
        # Quantity (also known as factor)
        # Value is 1 or larger. For tariffs it's always 1.
        # The field is from the charge link.
        StructField("Quantity", IntegerType(), False),

        # The start date of the link period. The start date must be the UTC time of the beginning of a date in the given timezone/DST.
        # The date is inclusive.
        StructField("FromDate", TimestampType(), False),
        
        # The to-date of the link period. The to-date must be the UTC time of the beginning of a date in the given timezone/DST.
        # The moment is exclusive.
        # All but the `ToDate` of the last period must have value. The `ToDate` of the last period can be null for subscriptions and tariffs.
        # The `ToDate` of fees is the day after the `FromDate`.
        StructField("ToDate", TimestampType(), True),
       
        # The year part of the `ToDate`. Used for partitioning.
        StructField("ToDate_Year", IntegerType(), True),
        
        # The month part of the `ToDate`. Used for partitioning.
        StructField("ToDate_Month", IntegerType(), True),
        
        # The day part (1-31) of the `ToDate`. Used for partitioning.
        StructField("ToDate_Day", IntegerType(), True),
    ]
)
