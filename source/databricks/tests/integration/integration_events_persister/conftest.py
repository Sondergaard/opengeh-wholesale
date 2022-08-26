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

import os
import shutil
import pytest
from pyspark.sql import SparkSession

from pyspark.sql.types import (
    StructType,
    StructField,
    StringType,
    DecimalType,
    IntegerType,
    TimestampType,
    BooleanType,
    BinaryType,
    LongType,
)

time_series_received_schema = StructType(
    [
        StructField("enqueuedTime", TimestampType(), True),
        StructField("body", StringType(), True),
    ]
)


@pytest.fixture(scope="session")
def parquet_reader(spark: SparkSession, data_lake_path: str):
    def f(path: str):
        data = spark.sparkContext.emptyRDD()
        try:
            data = spark.read.format("parquet").load(f"{data_lake_path}/{path}")
            data.show(truncate=False)
        except Exception:
            pass
        return data

    return f
