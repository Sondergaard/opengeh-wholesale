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

import pytest
from decimal import Decimal
import pandas as pd
from datetime import datetime, timedelta
from package.constants import Colname
from package.steps.aggregation import (
    aggregate_net_exchange_per_neighbour_ga,
)
from package.codelists import MeteringPointType, TimeSeriesQuality
from package.schemas.output import aggregation_result_schema
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
from pyspark.sql.functions import col, window


e_20 = MeteringPointType.exchange.value
date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string
)
numberOfTestQuarters = 96
estimated_quality = TimeSeriesQuality.estimated.value

df_template = {
    Colname.grid_area: [],
    Colname.metering_point_type: [],
    Colname.to_grid_area: [],
    Colname.from_grid_area: [],
    Colname.quantity: [],
    Colname.observation_time: [],
    Colname.quality: [],
}


@pytest.fixture(scope="module")
def time_series_schema():
    return (
        StructType()
        .add(Colname.grid_area, StringType())
        .add(Colname.metering_point_type, StringType())
        .add(Colname.to_grid_area, StringType())
        .add(Colname.from_grid_area, StringType())
        .add(Colname.quantity, DecimalType(38))
        .add(Colname.observation_time, TimestampType())
        .add(Colname.quality, StringType())
    )


@pytest.fixture(scope="module")
def single_quarter_test_data(spark, time_series_schema):
    pandas_df = pd.DataFrame(df_template)
    pandas_df = add_row_of_data(
        pandas_df, "A", "A", "B", default_obs_time, Decimal("10")
    )
    pandas_df = add_row_of_data(
        pandas_df, "A", "A", "B", default_obs_time, Decimal("15")
    )
    pandas_df = add_row_of_data(
        pandas_df, "A", "B", "A", default_obs_time, Decimal("5")
    )
    pandas_df = add_row_of_data(
        pandas_df, "B", "B", "A", default_obs_time, Decimal("10")
    )
    pandas_df = add_row_of_data(
        pandas_df, "A", "A", "C", default_obs_time, Decimal("20")
    )
    pandas_df = add_row_of_data(
        pandas_df, "C", "C", "A", default_obs_time, Decimal("10")
    )
    pandas_df = add_row_of_data(
        pandas_df, "C", "C", "A", default_obs_time, Decimal("5")
    )
    return spark.createDataFrame(pandas_df, time_series_schema).withColumn(
        Colname.time_window, window(col(Colname.observation_time), "15 minutes")
    )


@pytest.fixture(scope="module")
def multi_quarter_test_data(spark, time_series_schema):
    pandas_df = pd.DataFrame(df_template)
    for i in range(numberOfTestQuarters):
        pandas_df = add_row_of_data(
            pandas_df,
            "A",
            "A",
            "B",
            default_obs_time + timedelta(minutes=i * 15),
            Decimal("10"),
        )
        pandas_df = add_row_of_data(
            pandas_df,
            "A",
            "A",
            "B",
            default_obs_time + timedelta(minutes=i * 15),
            Decimal("15"),
        )
        pandas_df = add_row_of_data(
            pandas_df,
            "A",
            "B",
            "A",
            default_obs_time + timedelta(minutes=i * 15),
            Decimal("5"),
        )
        pandas_df = add_row_of_data(
            pandas_df,
            "B",
            "B",
            "A",
            default_obs_time + timedelta(minutes=i * 15),
            Decimal("10"),
        )
        pandas_df = add_row_of_data(
            pandas_df,
            "A",
            "A",
            "C",
            default_obs_time + timedelta(minutes=i * 15),
            Decimal("20"),
        )
        pandas_df = add_row_of_data(
            pandas_df,
            "C",
            "C",
            "A",
            default_obs_time + timedelta(minutes=i * 15),
            Decimal("10"),
        )
        pandas_df = add_row_of_data(
            pandas_df,
            "C",
            "C",
            "A",
            default_obs_time + timedelta(minutes=i * 15),
            Decimal("5"),
        )
    return spark.createDataFrame(pandas_df, schema=time_series_schema).withColumn(
        Colname.time_window, window(col(Colname.observation_time), "15 minutes")
    )


def add_row_of_data(pandas_df, domain, in_domain, out_domain, timestamp, quantity):
    new_row = {
        Colname.grid_area: domain,
        Colname.metering_point_type: e_20,
        Colname.to_grid_area: in_domain,
        Colname.from_grid_area: out_domain,
        Colname.quantity: quantity,
        Colname.observation_time: timestamp,
        Colname.quality: estimated_quality,
    }
    return pandas_df.append(new_row, ignore_index=True)


def test_aggregate_net_exchange_per_neighbour_ga_single_hour(single_quarter_test_data):
    df = aggregate_net_exchange_per_neighbour_ga(single_quarter_test_data).orderBy(
        Colname.to_grid_area, Colname.from_grid_area, Colname.time_window
    )
    values = df.collect()
    assert df.count() == 4
    assert values[0][Colname.to_grid_area] == "A"
    assert values[1][Colname.from_grid_area] == "C"
    assert values[2][Colname.to_grid_area] == "B"
    assert values[0][Colname.sum_quantity] == Decimal("10")
    assert values[1][Colname.sum_quantity] == Decimal("5")
    assert values[2][Colname.sum_quantity] == Decimal("-10")
    assert values[3][Colname.sum_quantity] == Decimal("-5")


def test_aggregate_net_exchange_per_neighbour_ga_multi_hour(multi_quarter_test_data):
    df = aggregate_net_exchange_per_neighbour_ga(multi_quarter_test_data).orderBy(
        Colname.to_grid_area, Colname.from_grid_area, Colname.time_window
    )
    values = df.collect()
    assert df.count() == 384
    assert values[0][Colname.to_grid_area] == "A"
    assert values[0][Colname.from_grid_area] == "B"
    assert (
        values[0][Colname.time_window][Colname.start].strftime(
            date_time_formatting_string
        )
        == "2020-01-01T00:00:00"
    )
    assert (
        values[0][Colname.time_window][Colname.end].strftime(
            date_time_formatting_string
        )
        == "2020-01-01T00:15:00"
    )
    assert values[0][Colname.sum_quantity] == Decimal("10")
    assert values[19][Colname.to_grid_area] == "A"
    assert values[19][Colname.from_grid_area] == "B"
    assert (
        values[19][Colname.time_window][Colname.start].strftime(
            date_time_formatting_string
        )
        == "2020-01-01T04:45:00"
    )
    assert (
        values[19][Colname.time_window][Colname.end].strftime(
            date_time_formatting_string
        )
        == "2020-01-01T05:00:00"
    )
    assert values[19][Colname.sum_quantity] == Decimal("10")


def test_expected_schema(single_quarter_test_data):
    df = aggregate_net_exchange_per_neighbour_ga(single_quarter_test_data).orderBy(
        Colname.to_grid_area, Colname.from_grid_area, Colname.time_window
    )
    assert df.schema == aggregation_result_schema
