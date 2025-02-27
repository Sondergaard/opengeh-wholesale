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
import pytest

from . import configuration as C
from package.codelists import (
    AggregationLevel,
    TimeSeriesType,
)
from package.constants import ResultTableColName

ALL_ENERGY_RESULT_TYPES = {
    (
        TimeSeriesType.NET_EXCHANGE_PER_NEIGHBORING_GA.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.NET_EXCHANGE_PER_GA.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        AggregationLevel.es_per_brp_per_ga.value,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        AggregationLevel.es_per_ga.value,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        AggregationLevel.brp_per_ga.value,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        AggregationLevel.es_per_brp_per_ga.value,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        AggregationLevel.es_per_ga.value,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        AggregationLevel.brp_per_ga.value,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        AggregationLevel.es_per_brp_per_ga.value,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        AggregationLevel.es_per_ga.value,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        AggregationLevel.brp_per_ga.value,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.GRID_LOSS.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.POSITIVE_GRID_LOSS.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.NEGATIVE_GRID_LOSS.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.TOTAL_CONSUMPTION.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.TEMP_FLEX_CONSUMPTION.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.TEMP_PRODUCTION.value,
        AggregationLevel.total_ga.value,
    ),
}

WHOLESALE_FIXING_ENERGY_RESULT_TYPES = {
    (
        TimeSeriesType.NET_EXCHANGE_PER_GA.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        AggregationLevel.es_per_ga.value,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        AggregationLevel.es_per_ga.value,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        AggregationLevel.es_per_ga.value,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.GRID_LOSS.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.POSITIVE_GRID_LOSS.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.NEGATIVE_GRID_LOSS.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.TOTAL_CONSUMPTION.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.TEMP_FLEX_CONSUMPTION.value,
        AggregationLevel.total_ga.value,
    ),
    (
        TimeSeriesType.TEMP_PRODUCTION.value,
        AggregationLevel.total_ga.value,
    ),
}


@pytest.mark.parametrize(
    "time_series_type, aggregation_level", ALL_ENERGY_RESULT_TYPES,
)
def test__balance_fixing_result__is_created(
    balance_fixing_results_df: DataFrame,
    time_series_type: str,
    aggregation_level: str,
) -> None:
    # Arrange
    result_df = (
        balance_fixing_results_df.where(F.col(ResultTableColName.batch_id) == C.executed_balance_fixing_batch_id)
        .where(F.col(ResultTableColName.time_series_type) == time_series_type)
        .where(F.col(ResultTableColName.aggregation_level) == aggregation_level)
    )

    # Act: Calculator job is executed just once per session. See the fixtures `balance_fixing_results_df` and `executed_balance_fixing`

    # Assert: The result is created if there are rows
    assert result_df.count() > 0


def test__balance_fixing_result__has_expected_number_of_result_types(
    balance_fixing_results_df: DataFrame,
) -> None:
    # Arrange
    actual_result_type_count = (
        balance_fixing_results_df.where(F.col(ResultTableColName.batch_id) == C.executed_balance_fixing_batch_id)
        .where(F.col(ResultTableColName.batch_id) == C.executed_balance_fixing_batch_id)
        .select(ResultTableColName.time_series_type, ResultTableColName.aggregation_level).distinct().count()
    )

    # Act: Calculator job is executed just once per session. See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert actual_result_type_count == len(ALL_ENERGY_RESULT_TYPES)


@pytest.mark.parametrize(
    "time_series_type, aggregation_level", WHOLESALE_FIXING_ENERGY_RESULT_TYPES,
)
def test__wholesale_result__is_created(
    wholesale_fixing_results_df: DataFrame,
    time_series_type: str,
    aggregation_level: str,
) -> None:
    # Arrange
    result_df = (
        wholesale_fixing_results_df.where(F.col(ResultTableColName.batch_id) == C.executed_wholesale_batch_id)
        .where(F.col(ResultTableColName.time_series_type) == time_series_type)
        .where(F.col(ResultTableColName.aggregation_level) == aggregation_level)
    )

    # Act: Calculator job is executed just once per session. See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert result_df.count() > 0


def test__wholesale_result__has_expected_number_of_result_types(
    wholesale_fixing_results_df: DataFrame,
) -> None:
    # Arrange
    actual_result_type_count = (
        wholesale_fixing_results_df.where(F.col(ResultTableColName.batch_id) == C.executed_wholesale_batch_id)
        .where(F.col(ResultTableColName.batch_id) == C.executed_wholesale_batch_id)
        .select(ResultTableColName.time_series_type, ResultTableColName.aggregation_level).distinct().count()
    )

    # Act: Calculator job is executed just once per session. See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert actual_result_type_count == len(WHOLESALE_FIXING_ENERGY_RESULT_TYPES)
