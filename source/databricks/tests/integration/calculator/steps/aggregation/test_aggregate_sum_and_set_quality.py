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

from package.codelists import TimeSeriesQuality
from package.constants import Colname
from package.steps.aggregation import __aggregate_sum_and_set_quality
from pyspark.sql import DataFrame, SparkSession

DUMMY_COL_NAME_1 = "dummy1"
DUMMY_COL_NAME_2 = "dummy2"
DUMMY_COL_NAME_3 = "dummy3"
QUANTITY_COL_NAME = "my_quantity"


def _create_input_row(
    quantity: str,
    quality: TimeSeriesQuality,
    dummy_col_1: str,
    dummy_col_2: str,
) -> dict:
    row = {
        QUANTITY_COL_NAME: Decimal(quantity),
        Colname.quality: quality.value,
        DUMMY_COL_NAME_1: dummy_col_1,
        DUMMY_COL_NAME_2: dummy_col_2,
    }

    return row


def test__aggregate_sum_and_set_quality__when_input_is_all_measured__sum_is_measured(
    spark: SparkSession,
) -> None:

    # Arrange
    rows = []
    rows.append(_create_input_row("1.1", TimeSeriesQuality.measured, "es_1", "brp_1"))
    rows.append(_create_input_row("2.1", TimeSeriesQuality.measured, "es_1", "brp_1"))
    rows.append(_create_input_row("3.1", TimeSeriesQuality.measured, "es_1", "brp_1"))

    input_df = spark.createDataFrame(data=rows)

    group_by = [DUMMY_COL_NAME_1, DUMMY_COL_NAME_2]

    # Act
    result = __aggregate_sum_and_set_quality(
        input_df,
        QUANTITY_COL_NAME,
        group_by,
    )

    # Assert
    result_collect = result.collect()
    assert len(result_collect) == 1
    assert result_collect[0][Colname.sum_quantity] == Decimal("6.3")
    assert result_collect[0][Colname.quality] == TimeSeriesQuality.measured.value
    assert result_collect[0][DUMMY_COL_NAME_1] == "es_1"
    assert result_collect[0][DUMMY_COL_NAME_2] == "brp_1"


def test__aggregate_sum_and_set_quality__when_A05_is_present__return_A05(
    spark: SparkSession,
) -> None:

    # Arrange
    rows = []
    rows.append(_create_input_row("1.1", TimeSeriesQuality.measured, "es_1", "brp_1"))
    rows.append(_create_input_row("2.1", TimeSeriesQuality.missing, "es_1", "brp_2"))
    rows.append(_create_input_row("3.1", TimeSeriesQuality.measured, "es_1", "brp_2"))
    rows.append(_create_input_row("1.1", TimeSeriesQuality.estimated, "es_1", "brp_2"))
    rows.append(_create_input_row("1.1", TimeSeriesQuality.incomplete, "es_1", "brp_2"))

    input_df = spark.createDataFrame(data=rows)

    group_by = [DUMMY_COL_NAME_1, DUMMY_COL_NAME_2]

    # Act
    result = __aggregate_sum_and_set_quality(
        input_df,
        QUANTITY_COL_NAME,
        group_by,
    )

    # Assert
    result_collect = result.collect()
    assert len(result_collect) == 2
    assert result_collect[0][Colname.quality] == TimeSeriesQuality.measured.value
    assert result_collect[1][Colname.quality] == TimeSeriesQuality.incomplete.value


def test__aggregate_sum_and_set_quality__when_A02_is_present__return_A05(
    spark: SparkSession,
) -> None:

    # Arrange
    rows = []
    rows.append(_create_input_row("1.1", TimeSeriesQuality.missing, "es_1", "brp_1"))
    rows.append(_create_input_row("2.1", TimeSeriesQuality.estimated, "es_1", "brp_1"))
    rows.append(_create_input_row("1.1", TimeSeriesQuality.measured, "es_1", "brp_1"))

    input_df = spark.createDataFrame(data=rows)

    group_by = [DUMMY_COL_NAME_1, DUMMY_COL_NAME_2]

    # Act
    result = __aggregate_sum_and_set_quality(
        input_df,
        QUANTITY_COL_NAME,
        group_by,
    )

    # Assert
    result_collect = result.collect()
    assert len(result_collect) == 1
    assert result_collect[0][Colname.quality] == TimeSeriesQuality.incomplete.value


def test__aggregate_sum_and_set_quality_A03_plus_A04_returns_A03(
    spark: SparkSession,
) -> None:

    # Arrange
    rows = []
    rows.append(_create_input_row("1.1", TimeSeriesQuality.estimated, "es_1", "brp_1"))
    rows.append(_create_input_row("2.1", TimeSeriesQuality.measured, "es_1", "brp_1"))

    input_df = spark.createDataFrame(data=rows)

    group_by = [DUMMY_COL_NAME_1, DUMMY_COL_NAME_2]

    # Act
    result = __aggregate_sum_and_set_quality(
        input_df,
        QUANTITY_COL_NAME,
        group_by,
    )

    # Assert
    result_collect = result.collect()
    assert len(result_collect) == 1
    assert result_collect[0][Colname.quality] == TimeSeriesQuality.estimated.value


def test__aggregate_sum_and_set_quality__A03_plus_A03__returns_A03(
    spark: SparkSession,
) -> None:

    # Arrange
    rows = []
    rows.append(_create_input_row("1.1", TimeSeriesQuality.estimated, "es_1", "brp_1"))
    rows.append(_create_input_row("2.1", TimeSeriesQuality.estimated, "es_1", "brp_1"))

    input_df = spark.createDataFrame(data=rows)

    group_by = [DUMMY_COL_NAME_1, DUMMY_COL_NAME_2]

    # Act
    result = __aggregate_sum_and_set_quality(
        input_df,
        QUANTITY_COL_NAME,
        group_by,
    )

    # Assert
    result_collect = result.collect()
    assert len(result_collect) == 1
    assert result_collect[0][Colname.quality] == TimeSeriesQuality.estimated.value


def test__aggregate_sum_and_set_quality__A04_plus_A04__returns_A04(
    spark: SparkSession,
) -> None:

    # Arrange
    rows = []
    rows.append(_create_input_row("1.1", TimeSeriesQuality.measured, "es_1", "brp_1"))
    rows.append(_create_input_row("2.1", TimeSeriesQuality.measured, "es_1", "brp_1"))

    input_df = spark.createDataFrame(data=rows)

    group_by = [DUMMY_COL_NAME_1, DUMMY_COL_NAME_2]

    # Act
    result = __aggregate_sum_and_set_quality(
        input_df,
        QUANTITY_COL_NAME,
        group_by,
    )

    # Assert
    result_collect = result.collect()
    assert len(result_collect) == 1
    assert result_collect[0][Colname.quality] == TimeSeriesQuality.measured.value


def test__aggregate_sum_and_set_quality__sum_only_within_group(
    spark: SparkSession,
) -> None:

    # Arrange
    rows = []
    rows.append(_create_input_row("1.1", TimeSeriesQuality.measured, "es_1", "brp_1"))
    rows.append(_create_input_row("2.1", TimeSeriesQuality.measured, "es_1", "brp_1"))

    rows.append(_create_input_row("3.1", TimeSeriesQuality.measured, "es_2", "brp_1"))
    rows.append(_create_input_row("4.1", TimeSeriesQuality.measured, "es_2", "brp_1"))

    rows.append(_create_input_row("5.1", TimeSeriesQuality.measured, "es_2", "brp_2"))
    rows.append(_create_input_row("6.1", TimeSeriesQuality.measured, "es_2", "brp_2"))

    input_df = spark.createDataFrame(data=rows)

    group_by = [DUMMY_COL_NAME_1, DUMMY_COL_NAME_2]

    # Act
    result = __aggregate_sum_and_set_quality(
        input_df,
        QUANTITY_COL_NAME,
        group_by,
    )

    # Assert
    result_collect = result.collect()
    assert len(result_collect) == 3
    assert result_collect[0][Colname.sum_quantity] == Decimal("3.2")
    assert result_collect[1][Colname.sum_quantity] == Decimal("7.2")
    assert result_collect[2][Colname.sum_quantity] == Decimal("11.2")


def test__aggregate_sum_and_set_quality__compare_quality_only_within_group(
    spark: SparkSession,
) -> None:

    # Arrange
    rows = []
    rows.append(_create_input_row("1.1", TimeSeriesQuality.measured, "es_1", "brp_1"))
    rows.append(_create_input_row("2.1", TimeSeriesQuality.missing, "es_1", "brp_1"))

    rows.append(_create_input_row("3.1", TimeSeriesQuality.measured, "es_2", "brp_1"))
    rows.append(_create_input_row("4.1", TimeSeriesQuality.estimated, "es_2", "brp_1"))

    rows.append(_create_input_row("5.1", TimeSeriesQuality.measured, "es_2", "brp_2"))
    rows.append(_create_input_row("6.1", TimeSeriesQuality.measured, "es_2", "brp_2"))

    input_df = spark.createDataFrame(data=rows)

    group_by = [DUMMY_COL_NAME_1, DUMMY_COL_NAME_2]

    # Act
    result = __aggregate_sum_and_set_quality(
        input_df,
        QUANTITY_COL_NAME,
        group_by,
    )

    # Assert
    result_collect = result.collect()
    assert len(result_collect) == 3
    assert result_collect[0][Colname.quality] == TimeSeriesQuality.incomplete.value
    assert result_collect[1][Colname.quality] == TimeSeriesQuality.estimated.value
    assert result_collect[2][Colname.quality] == TimeSeriesQuality.measured.value
    