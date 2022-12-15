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
from decimal import Decimal
from datetime import datetime
from package.constants import Colname, ResultKeyName
from package.steps.aggregation import (
    aggregate_production,
    aggregate_per_ga_and_brp_and_es,
)
from package.codelists import (
    MeteringPointType,
    MeteringPointResolution,
    TimeSeriesQuality,
)

from package.codelists import ConnectionState
from package.shared.data_classes import Metadata
from package.schemas.output import aggregation_result_schema
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, sum
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
import pytest
import pandas as pd

e_17 = MeteringPointType.consumption.value
e_18 = MeteringPointType.production.value
e_20 = MeteringPointType.exchange.value

# Default time series data point values
default_point_type = e_18
default_grid_area = "G1"
default_responsible = "R1"
default_supplier = "S1"
default_quantity = Decimal(1)
default_connection_state = ConnectionState.connected.value
default_resolution = "PT15M"

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string
)

metadata = Metadata("1", "1", "1", "1", "1")


@pytest.fixture(scope="module")
def time_series_schema():
    """
    Input time series data point schema
    """
    return (
        StructType()
        .add(Colname.metering_point_type, StringType(), False)
        .add(Colname.grid_area, StringType(), False)
        .add(Colname.balance_responsible_id, StringType())
        .add(Colname.energy_supplier_id, StringType())
        .add(Colname.quantity, DecimalType())
        .add(Colname.time, TimestampType())
        .add(Colname.connection_state, StringType())
        .add(Colname.quality, StringType())
        .add(Colname.resolution, StringType())
    )


@pytest.fixture(scope="module")
def time_series_row_factory(spark, time_series_schema):
    """
    Factory to generate a single row of time series data, with default parameters as specified above.
    """

    def factory(
        point_type=default_point_type,
        grid_area=default_grid_area,
        responsible=default_responsible,
        supplier=default_supplier,
        quantity=default_quantity,
        obs_time=default_obs_time,
        connection_state=default_connection_state,
        resolution=default_resolution,
    ):
        pandas_df = pd.DataFrame(
            {
                Colname.metering_point_type: [point_type],
                Colname.grid_area: [grid_area],
                Colname.balance_responsible_id: [responsible],
                Colname.energy_supplier_id: [supplier],
                Colname.quantity: [quantity],
                Colname.time: [obs_time],
                Colname.connection_state: [connection_state],
                Colname.quality: [TimeSeriesQuality.estimated.value],
                Colname.resolution: [resolution],
            }
        )
        return spark.createDataFrame(pandas_df, schema=time_series_schema)

    return factory


def check_aggregation_row(
    df: DataFrame,
    row: int,
    grid: str,
    responsible: str,
    supplier: str,
    sum: Decimal,
    start: datetime,
    end: datetime,
):
    """
    Helper function that checks column values for the given row.
    Note that start and end datetimes are timezone-naive - we set the Spark session timezone to UTC in the
    conftest.py, since:
        From https://stackoverflow.com/questions/48746376/timestamptype-in-pyspark-with-datetime-tzaware-objects:
        "TimestampType in pyspark is not tz aware like in Pandas rather it passes long ints
        and displays them according to your machine's local time zone (by default)"
    """
    pandas_df = df.toPandas()
    assert pandas_df[Colname.grid_area][row] == grid
    assert pandas_df[Colname.balance_responsible_id][row] == responsible
    assert pandas_df[Colname.energy_supplier_id][row] == supplier
    assert pandas_df[Colname.sum_quantity][row] == sum
    assert pandas_df[Colname.time_window][row].start == start
    assert pandas_df[Colname.time_window][row].end == end


@pytest.mark.parametrize(
    "point_type",
    [
        pytest.param(e_17, id="invalid because metering point type is consumption"),
        pytest.param(e_20, id="invalid because metering point type is exchange"),
    ],
)
def test_production_aggregator_filters_out_incorrect_point_type(
    point_type, time_series_row_factory
):
    """
    Aggregator should filter out all non "E18" MarketEvaluationPointType rows
    """
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = time_series_row_factory(
        point_type=point_type
    )
    aggregated_df = aggregate_production(results, metadata)
    assert aggregated_df.count() == 0


def test_production_aggregator_aggregates_observations_in_same_quarter_hour(
    time_series_row_factory,
):
    """
    Aggregator should calculate the correct sum of a "grid area" grouping within the
    same quarter hour time window
    """
    row1_df = time_series_row_factory(quantity=Decimal(1))
    row2_df = time_series_row_factory(quantity=Decimal(2))
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = row1_df.union(row2_df)
    aggregated_df = aggregate_production(results, metadata)

    # Create the start/end datetimes representing the start and end of the 1 hr time period
    # These should be datetime naive in order to compare to the Spark Dataframe
    start_time = datetime(2020, 1, 1, 0, 0, 0)
    end_time = datetime(2020, 1, 1, 0, 15, 0)

    assert aggregated_df.count() == 1
    check_aggregation_row(
        aggregated_df,
        0,
        default_grid_area,
        default_responsible,
        default_supplier,
        Decimal(3),
        start_time,
        end_time,
    )


def test_production_aggregator_returns_distinct_rows_for_observations_in_different_hours(
    time_series_row_factory,
):
    """
    Aggregator can calculate the correct sum of a "grid area"-"responsible"-"supplier" grouping
    within the 2 different quarter hour time windows
    """
    diff_obs_time = datetime.strptime(
        "2020-01-01T01:00:00+0000", date_time_formatting_string
    )

    row1_df = time_series_row_factory()
    row2_df = time_series_row_factory(obs_time=diff_obs_time)
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = row1_df.union(row2_df)
    aggregated_df = aggregate_production(results, metadata)

    assert aggregated_df.count() == 2

    # Create the start/end datetimes representing the start and end of the quarter hour time period for each row's ObservationTime
    # These should be datetime naive in order to compare to the Spark Dataframe
    start_time_row1 = datetime(2020, 1, 1, 0, 0, 0)
    end_time_row1 = datetime(2020, 1, 1, 0, 15, 0)
    check_aggregation_row(
        aggregated_df,
        0,
        default_grid_area,
        default_responsible,
        default_supplier,
        default_quantity,
        start_time_row1,
        end_time_row1,
    )

    start_time_row2 = datetime(2020, 1, 1, 1, 0, 0)
    end_time_row2 = datetime(2020, 1, 1, 1, 15, 0)
    check_aggregation_row(
        aggregated_df,
        1,
        default_grid_area,
        default_responsible,
        default_supplier,
        default_quantity,
        start_time_row2,
        end_time_row2,
    )


def test_production_aggregator_returns_correct_schema(time_series_row_factory):
    """
    Aggregator should return the correct schema, including the proper fields for the aggregated quantity values
    and time window (from the quarter-hour resolution specified in the aggregator).
    """
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = time_series_row_factory()
    aggregated_df = aggregate_production(results, metadata)
    assert aggregated_df.schema == aggregation_result_schema


def test_production_test_filter_by_domain_is_pressent(time_series_row_factory):
    df = time_series_row_factory()
    aggregated_df = aggregate_per_ga_and_brp_and_es(
        df, MeteringPointType.production, None, metadata
    )
    assert aggregated_df.count() == 1


minimum_quantity = Decimal("0.001")
grid_area_code_805 = "805"
grid_area_code_806 = "806"


@pytest.fixture
def enriched_time_series_quarterly_same_time_factory(spark, timestamp_factory):
    def factory(
        first_resolution=MeteringPointResolution.quarter.value,
        second_resolution=MeteringPointResolution.quarter.value,
        first_quantity=Decimal("1"),
        second_quantity=Decimal("2"),
        first_time="2022-06-08T12:09:15.000Z",
        second_time="2022-06-08T12:09:15.000Z",
        first_grid_area_code=grid_area_code_805,
        second_grid_area_code=grid_area_code_805,
    ):
        time = timestamp_factory(first_time)
        time2 = timestamp_factory(second_time)

        df = [
            {
                Colname.metering_point_type: MeteringPointType.production.value,
                Colname.grid_area: first_grid_area_code,
                Colname.balance_responsible_id: default_responsible,
                Colname.energy_supplier_id: default_supplier,
                Colname.resolution: first_resolution,
                Colname.time: time,
                Colname.quantity: first_quantity,
                Colname.quality: TimeSeriesQuality.measured.value,
            },
            {
                Colname.metering_point_type: MeteringPointType.production.value,
                "GridAreaCode": second_grid_area_code,
                Colname.balance_responsible_id: default_responsible,
                Colname.energy_supplier_id: default_supplier,
                "Resolution": second_resolution,
                "time": time2,
                Colname.quantity: second_quantity,
                "Quality": TimeSeriesQuality.measured.value,
            },
        ]

        return spark.createDataFrame(df)

    return factory


@pytest.fixture
def enriched_time_series_factory(spark, timestamp_factory):
    def factory(
        resolution=MeteringPointResolution.quarter.value,
        quantity=Decimal("1"),
        quality=TimeSeriesQuality.measured.value,
        gridArea="805",
    ):
        time = timestamp_factory("2022-06-08T12:09:15.000Z")

        df = [
            {
                Colname.metering_point_type: MeteringPointType.production.value,
                Colname.grid_area: gridArea,
                Colname.balance_responsible_id: default_responsible,
                Colname.energy_supplier_id: default_supplier,
                Colname.resolution: resolution,
                Colname.time: time,
                Colname.quantity: quantity,
                Colname.quality: quality,
            }
        ]
        return spark.createDataFrame(df)

    return factory


# Test sums with only quarterly can be calculated
def test__quarterly_sums_correctly(
    enriched_time_series_quarterly_same_time_factory,
):
    """Test that checks quantity is summed correctly with only quarterly times"""
    df = enriched_time_series_quarterly_same_time_factory(
        first_quantity=Decimal("1"), second_quantity=Decimal("2")
    )
    result_df = aggregate_per_ga_and_brp_and_es(
        df, MeteringPointType.production, None, metadata
    )
    result_df.show()
    assert result_df.first().sum_quantity == 3


@pytest.mark.parametrize(
    "quantity, expected_point_quantity",
    [
        # 0.001 / 4 = 0.000250 ≈ 0.000
        (Decimal("0.001"), Decimal("0.000")),
        # 0.002 / 4 = 0.000500 ≈ 0.001
        (Decimal("0.002"), Decimal("0.001")),
        # 0.003 / 4 = 0.000750 ≈ 0.001
        (Decimal("0.003"), Decimal("0.001")),
        # 0.004 / 4 = 0.001000 ≈ 0.001
        (Decimal("0.004"), Decimal("0.001")),
        # 0.005 / 4 = 0.001250 ≈ 0.001
        (Decimal("0.005"), Decimal("0.001")),
        # 0.006 / 4 = 0.001500 ≈ 0.002
        (Decimal("0.006"), Decimal("0.002")),
        # 0.007 / 4 = 0.001750 ≈ 0.002
        (Decimal("0.007"), Decimal("0.002")),
        # 0.008 / 4 = 0.002000 ≈ 0.002
        (Decimal("0.008"), Decimal("0.002")),
    ],
)
def test__hourly_sums_are_rounded_correctly(
    enriched_time_series_factory, quantity, expected_point_quantity
):
    """Test that checks acceptable rounding erros for hourly quantities summed on a quarterly basis"""
    df = enriched_time_series_factory(
        resolution=MeteringPointResolution.hour.value, quantity=quantity
    )

    result_df = aggregate_per_ga_and_brp_and_es(
        df, MeteringPointType.production, None, metadata
    )

    assert result_df.count() == 4  # one hourly quantity should yield 4 points
    assert (
        result_df.where(col(Colname.sum_quantity) == expected_point_quantity).count()
        == 4
    )


def test__quarterly_and_hourly_sums_correctly(
    enriched_time_series_quarterly_same_time_factory,
):
    """Test that checks quantity is summed correctly with quarterly and hourly times"""
    first_quantity = Decimal("4")
    second_quantity = Decimal("2")
    df = enriched_time_series_quarterly_same_time_factory(
        first_resolution=MeteringPointResolution.quarter.value,
        first_quantity=first_quantity,
        second_resolution=MeteringPointResolution.hour.value,
        second_quantity=second_quantity,
    )
    result_df = aggregate_per_ga_and_brp_and_es(
        df, MeteringPointType.production, None, metadata
    )
    result_df.printSchema()
    result_df.show()
    sum_quant = result_df.agg(sum(Colname.sum_quantity).alias("sum_quant"))
    assert sum_quant.first()["sum_quant"] == first_quantity + second_quantity
