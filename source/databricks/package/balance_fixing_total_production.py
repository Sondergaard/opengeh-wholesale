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
    array,
    lit,
    col,
    from_json,
    row_number,
    expr,
    when,
    lead,
    last,
    coalesce,
    explode,
)
from pyspark.sql.types import (
    IntegerType,
    StructField,
    StringType,
    TimestampType,
    StructType,
)
from pyspark.sql.window import Window
from package.codelists import ConnectionState, MeteringPointType, Resolution


def calculate_balance_fixing_total_production(
    raw_integration_events_df,
    raw_time_series_points,
    batch_id,
    batch_grid_areas,
    snapshot_datetime,
    period_start_datetime,
    period_end_datetime,
) -> DataFrame:
    grid_area_df = _get_grid_areas_df(
        raw_integration_events_df, batch_grid_areas, snapshot_datetime
    )
    metering_point_period_df = _get_metering_point_periods_df(
        raw_integration_events_df,
        grid_area_df,
        snapshot_datetime,
        period_start_datetime,
        period_end_datetime,
    )
    enriched_time_series_point_df = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        snapshot_datetime,
        period_start_datetime,
        period_end_datetime,
    )
    result_df = _get_result_df(enriched_time_series_point_df, batch_grid_areas)

    return result_df


def _get_grid_areas_df(
    raw_integration_events_df, batch_grid_areas, snapshot_datetime
) -> DataFrame:
    grid_area_event_schema = StructType(
        [
            StructField("GridAreaCode", StringType(), True),
            StructField("GridAreaLinkId", StringType(), True),
            StructField("MessageType", StringType(), True),
        ]
    )
    grid_area_events_df = (
        raw_integration_events_df.where(col("storedTime") <= snapshot_datetime)
        .withColumn("body", col("body").cast("string"))
        .withColumn("body", from_json(col("body"), grid_area_event_schema))
        .where(col("body.MessageType") == "GridAreaUpdatedIntegrationEvent")
        .where(col("body.GridAreaCode").isin(batch_grid_areas))
    )

    # As we only use (currently) immutable data we can just pick any of the update events randomly.
    # This will, however, change when support for merge of grid areas are added.
    w2 = Window.partitionBy("body.GridAreaCode").orderBy(
        col("storedTime")  # TODO: should be operation timestamp
    )
    # only get nevest events
    grid_area_events_df = (
        grid_area_events_df.withColumn(
            "row", row_number().over(w2)
        )  # orderby storedTime and add row number
        .filter(col("row") == 1)  # only take newest event
        .drop("row")
        .select("body.GridAreaLinkId", "body.GridAreaCode")
    )

    if grid_area_events_df.count() != len(batch_grid_areas):
        grid_area_events_df.show()
        raise Exception(
            "Grid areas for processes in batch does not match the known grid areas in wholesale"
        )

    return grid_area_events_df


def _get_metering_point_periods_df(
    raw_integration_events_df,
    grid_area_df,
    snapshot_datetime,
    period_start_datetime,
    period_end_datetime,
) -> DataFrame:
    schema = StructType(
        [
            StructField("GsrnNumber", StringType(), True),
            StructField("GridAreaLinkId", StringType(), True),
            StructField("ConnectionState", StringType(), True),
            StructField("EffectiveDate", TimestampType(), True),
            StructField("MeteringPointType", StringType(), True),
            StructField("MeteringPointId", StringType(), True),
            StructField("Resolution", StringType(), True),
            StructField("MessageType", StringType(), True),
        ]
    )

    metering_point_events_df = (
        raw_integration_events_df.where(col("storedTime") <= snapshot_datetime)
        .withColumn("body", col("body").cast("string"))
        .withColumn("body", from_json(col("body"), schema))
        .where(col("body.MessageType").startswith("MeteringPoint"))
        .select(
            "storedTime",
            "body.MessageType",
            "body.MeteringPointId",
            "body.MeteringPointType",
            "body.GsrnNumber",
            "body.GridAreaLinkId",
            "body.ConnectionState",
            "body.EffectiveDate",
            "body.Resolution",
        )
    )

    window = Window.partitionBy("MeteringPointId").orderBy("EffectiveDate")

    metering_point_periods_df = (
        metering_point_events_df.withColumn(
            "toEffectiveDate",
            lead("EffectiveDate", 1, "2099-01-01T23:00:00.000+0000").over(window),
        )
        .withColumn(
            "GridAreaLinkId",
            coalesce(col("GridAreaLinkId"), last("GridAreaLinkId", True).over(window)),
        )
        .withColumn(
            "ConnectionState",
            when(
                col("MessageType") == "MeteringPointCreated",
                lit(ConnectionState.new),
            ).when(
                col("MessageType") == "MeteringPointConnected",
                lit(ConnectionState.connected),
            ),
        )
        .withColumn(
            "MeteringPointType",
            coalesce(
                col("MeteringPointType"), last("MeteringPointType", True).over(window)
            ),
        )
        .withColumn(
            "Resolution",
            coalesce(col("Resolution"), last("Resolution", True).over(window)),
        )
        .where(col("EffectiveDate") <= period_end_datetime)
        .where(col("toEffectiveDate") >= period_start_datetime)
        .where(
            col("ConnectionState") == ConnectionState.connected
        )  # Only aggregate when metering points is connected
        .where(col("MeteringPointType") == MeteringPointType.production)
    )

    # Only include metering points in the selected grid areas
    metering_point_periods_df = metering_point_periods_df.join(
        grid_area_df,
        metering_point_periods_df["GridAreaLinkId"] == grid_area_df["GridAreaLinkId"],
        "inner",
    ).select(
        metering_point_periods_df["MessageType"],
        "GsrnNumber",
        "GridAreaCode",
        "EffectiveDate",
        "toEffectiveDate",
        "Resolution",
    )

    return metering_point_periods_df


def _get_enriched_time_series_points_df(
    raw_time_series_points,
    metering_point_period_df,
    snapshot_datetime,
    period_start_datetime,
    period_end_datetime,
) -> DataFrame:
    timeseries_df = (
        raw_time_series_points.where(col("storedTime") <= snapshot_datetime)
        .where(col("time") >= period_start_datetime)
        .where(col("time") < period_end_datetime)
        # Quantity of time series points should have 3 digits. Calculations, however, must use 6 digit precision to reduce rounding errors
        .withColumn("quantity", col("quantity").cast("decimal(18,6)"))
    )

    # Only use latest registered points
    window = Window.partitionBy("metering_point_id", "time").orderBy(
        col("registration_date_time").desc(), col("storedTime").desc()
    )
    timeseries_df = timeseries_df.withColumn(
        "row_number", row_number().over(window)
    ).where(col("row_number") == 1)

    timeseries_df = timeseries_df.select(
        col("metering_point_id").alias("GsrnNumber"), "time", "quantity"
    )

    # TODO: Use range join optimization: This query has a join condition that can benefit from range join optimization.
    #       To improve performance, consider adding a range join hint.
    #       https://docs.microsoft.com/azure/databricks/delta/join-performance/range-join
    enriched_time_series_point_df = timeseries_df.join(
        metering_point_period_df,
        (metering_point_period_df["GsrnNumber"] == timeseries_df["GsrnNumber"])
        & (timeseries_df["time"] >= metering_point_period_df["EffectiveDate"])
        & (timeseries_df["time"] < metering_point_period_df["toEffectiveDate"]),
        "inner",
    ).select(
        "GridAreaCode",
        metering_point_period_df["GsrnNumber"],
        "Resolution",
        "time",
        "quantity",
    )

    return enriched_time_series_point_df


def _get_result_df(enriched_time_series_points_df, batch_grid_areas) -> DataFrame:
    # TODO: Use range join optimization: This query has a join condition that can benefit from range join optimization.
    #       To improve performance, consider adding a range join hint.
    #       https://docs.microsoft.com/azure/databricks/delta/join-performance/range-join

    # Total production in batch grid areas with quarterly resolution as json file per grid area
    result_df = (
        enriched_time_series_points_df.where(col("GridAreaCode").isin(batch_grid_areas))
        .withColumn(
            "quarter_times",
            when(
                col("resolution") == Resolution.hour,
                array(
                    col("time"),
                    col("time") + expr("INTERVAL 15 minutes"),
                    col("time") + expr("INTERVAL 30 minutes"),
                    col("time") + expr("INTERVAL 45 minutes"),
                ),
            ).when(col("resolution") == Resolution.quarter, array(col("time"))),
        )
        .select(
            enriched_time_series_points_df["*"],
            explode("quarter_times").alias("quarter_time"),
        )
        .withColumn(
            "quarter_quantity",
            when(col("resolution") == Resolution.hour, col("quantity") / 4).when(
                col("resolution") == Resolution.quarter, col("quantity")
            ),
        )
        .groupBy("GridAreaCode", "quarter_time")
        .sum("quarter_quantity")
    )

    window = Window.partitionBy("grid-area").orderBy(col("quarter_time"))

    # TODO: Use range join optimization: This query has a join condition that can benefit from range join optimization.
    #       To improve performance, consider adding a range join hint.
    #       https://docs.microsoft.com/azure/databricks/delta/join-performance/range-join

    # Points may be missing in result time series if all metering points are missing a point at a certain moment.
    # According to PO and SME we can for now assume that full time series have been submitted for the processes/tests in question.
    result_df = (
        result_df.withColumnRenamed("GridAreaCode", "grid-area")
        .withColumn("position", row_number().over(window))
        .drop("quarter_time")
        .withColumnRenamed("sum(quarter_quantity)", "quantity")
    )

    return result_df
