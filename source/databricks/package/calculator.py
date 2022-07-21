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

from pyspark.sql.functions import lit, col
from pyspark.sql.types import IntegerType


def calculator(
    spark,
    raw_integration_events_df,
    process_results_path,
    batch_id,
):
    rdd = spark.sparkContext.parallelize(list(range(1, 97)))
    df_seq = spark.createDataFrame(rdd, schema=IntegerType()).withColumnRenamed(
        "value", "position"
    )

    integrationEventsDf = prepare_integration_events_df(raw_integration_events_df)

    df_805 = df_seq.withColumn("grid_area", lit("805"))
    df_806 = df_seq.withColumn("grid_area", lit("806"))

    df = df_805.union(df_806)

    df = df.withColumn("quantity", lit(None)).withColumn("quality", lit(None))
    df.coalesce(1).write.mode("overwrite").partitionBy("grid_area").json(
        f"{process_results_path}/batch_id={batch_id}"
    )

    integrationEventsDf.write.mode("overwrite").json(
        f"{process_results_path}/test/batch_id={batch_id}"
    )


def prepare_integration_events_df(raw_integration_events_df):
    gridAreaCodesDf = (
        raw_integration_events_df.filter(
            col("MessageType") == "GridAreaUpdatedIntegrationEvent"
        )
        .select(col("GridAreaCode"), col("GridAreaLinkId"))
        .distinct()
    )

    return (
        raw_integration_events_df.filter(
            col("MessageType") != "GridAreaUpdatedIntegrationEvent"
        )
        .drop(col("GridAreaCode"))
        .join(gridAreaCodesDf, "GridAreaLinkId")
    )
