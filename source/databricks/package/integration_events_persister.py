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
from pyspark.sql.functions import (
    year, month, dayofmonth, col, from_json, schema)


def integration_events_persister(
    streamingDf: DataFrame,
    checkpoint_path: str,
    integration_events_path: str
    ):
    events = (streamingDf.withColumn(
        "body", from_json(col("body").cast('string'), schema))
        .select(
            col("*"),
            col("body.*")
        ).drop("body")
        .withColumn("year", year(col("enqueuedTime")))
        .withColumn("month", month(col("enqueuedTime")))
        .withColumn("day", dayofmonth(col("enqueuedTime")))
        .stat()
        .awaitTermination())

    events.show()
