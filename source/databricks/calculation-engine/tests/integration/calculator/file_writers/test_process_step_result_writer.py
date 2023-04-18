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

from datetime import datetime
from decimal import Decimal
from pathlib import Path
from typing import List

import package.infrastructure as infra
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col
import pytest
import uuid

from package.codelists import (
    AggregationLevel,
    MeteringPointResolution,
    TimeSeriesQuality,
    TimeSeriesType,
)
from package.constants import Colname, ResultTableColName
from package.file_writers.process_step_result_writer import ProcessStepResultWriter
from tests.helpers.assert_calculation_file_path import (
    CalculationFileType,
    assert_file_path_match_contract,
)
from tests.helpers.file_utils import find_file
from typing import Any

DATABASE_NAME = "wholesale_output"
RESULT_TABLE_NAME = "result"
DEFAULT_BATCH_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_GRID_AREA = "105"
DEFAULT_TO_GRID_AREA = "106"
DEFAULT_ENERGY_SUPPLIER_ID = "9876543210123"
DEFAULT_BALANCE_RESPONSIBLE_ID = "1234567890123"
DEFAULT_PROCESS_TYPE = "BalanceFixing"
DEFAULT_BATCH_EXECUTION_START = datetime(2022, 6, 10, 13, 15)
DEFAULT_QUANTITY = "1.1"
DEFAULT_QUALITY = TimeSeriesQuality.measured
DEFAULT_TIME_SERIES_TYPE = TimeSeriesType.PRODUCTION
DEFAULT_AGGREGATION_LEVEL = AggregationLevel.total_ga

TABLE_NAME = f"{DATABASE_NAME}.{RESULT_TABLE_NAME}"


def _create_result_row(
    grid_area: str,
    to_grid_area: str,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str = DEFAULT_BALANCE_RESPONSIBLE_ID,
    quantity: str = DEFAULT_QUANTITY,
    quality: TimeSeriesQuality = DEFAULT_QUALITY,
) -> dict:
    row = {
        Colname.grid_area: grid_area,
        Colname.out_grid_area: to_grid_area,
        Colname.sum_quantity: Decimal(quantity),
        Colname.quality: quality.value,
        Colname.resolution: MeteringPointResolution.quarter.value,
        Colname.time_window: {
            Colname.start: datetime(2020, 1, 1, 0, 0),
            Colname.end: datetime(2020, 1, 1, 1, 0),
        },
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_id: balance_responsible_id,
    }

    return row


def _create_result_df(spark: SparkSession, row: List[dict]) -> DataFrame:
    return spark.createDataFrame(data=row).withColumn(
        Colname.sum_quantity, col(Colname.sum_quantity).cast("decimal(18, 3)")
    )


def test__write___when_aggregation_level_is_es_per_ga__result_file_path_matches_contract(
    spark: SparkSession,
    contracts_path: str,
    tmpdir: Path,
    migrations_executed: None,
) -> None:
    # Arrange
    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA,
            to_grid_area=DEFAULT_TO_GRID_AREA,
            energy_supplier_id=DEFAULT_ENERGY_SUPPLIER_ID,
        )
    ]
    result_df = _create_result_df(spark, row)
    relative_output_path = infra.get_result_file_relative_path(
        DEFAULT_BATCH_ID,
        DEFAULT_GRID_AREA,
        DEFAULT_ENERGY_SUPPLIER_ID,
        None,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.es_per_ga,
    )
    sut = ProcessStepResultWriter(
        str(tmpdir),
        DEFAULT_BATCH_ID,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )

    # Act
    sut.write(
        result_df,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.es_per_ga,
    )

    # Assert
    actual_result_file = find_file(
        f"{str(tmpdir)}/",
        f"{relative_output_path}/part-*.json",
    )
    assert_file_path_match_contract(
        contracts_path,
        actual_result_file,
        CalculationFileType.ResultFile,
    )


def test__write___when_aggregation_level_is_total_ga__result_file_path_matches_contract(
    spark: SparkSession,
    contracts_path: str,
    tmpdir: Path,
    migrations_executed: None,
) -> None:
    # Arrange
    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA,
            to_grid_area=DEFAULT_TO_GRID_AREA,
            energy_supplier_id="None",
        )
    ]
    result_df = _create_result_df(spark, row)
    relative_output_path = infra.get_result_file_relative_path(
        DEFAULT_BATCH_ID,
        DEFAULT_GRID_AREA,
        None,
        None,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.total_ga,
    )
    sut = ProcessStepResultWriter(
        str(tmpdir),
        DEFAULT_BATCH_ID,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )

    # Act
    sut.write(
        result_df,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.total_ga,
    )

    # Assert
    actual_result_file = find_file(
        f"{str(tmpdir)}/",
        f"{relative_output_path}/part-*.json",
    )
    assert_file_path_match_contract(
        contracts_path,
        actual_result_file,
        CalculationFileType.ResultFileForTotalGridArea,
    )


def test__write___when_aggregation_level_is_ga_brp_es__result_file_path_matches_contract(
    spark: SparkSession,
    contracts_path: str,
    tmpdir: Path,
    migrations_executed: None,
) -> None:
    # Arrange
    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA,
            to_grid_area=DEFAULT_TO_GRID_AREA,
            energy_supplier_id=DEFAULT_ENERGY_SUPPLIER_ID,
            balance_responsible_id=DEFAULT_BALANCE_RESPONSIBLE_ID,
        )
    ]
    result_df = _create_result_df(spark, row)
    relative_output_path = infra.get_result_file_relative_path(
        DEFAULT_BATCH_ID,
        DEFAULT_GRID_AREA,
        DEFAULT_ENERGY_SUPPLIER_ID,
        DEFAULT_BALANCE_RESPONSIBLE_ID,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.es_per_brp_per_ga,
    )
    sut = ProcessStepResultWriter(
        str(tmpdir),
        DEFAULT_BATCH_ID,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )

    # Act
    sut.write(
        result_df,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.es_per_brp_per_ga,
    )

    # Assert
    actual_result_file = find_file(
        f"{str(tmpdir)}/",
        f"{relative_output_path}/part-*.json",
    )
    assert_file_path_match_contract(
        contracts_path,
        actual_result_file,
        CalculationFileType.ResultFileForGaBrpEs,
    )


@pytest.mark.parametrize(
    "aggregation_level",
    AggregationLevel,
)
def test__write__writes_aggregation_level(
    spark: SparkSession,
    tmpdir: Path,
    aggregation_level: AggregationLevel,
    migrations_executed: None,
) -> None:
    # Arrange
    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA,
            to_grid_area=DEFAULT_TO_GRID_AREA,
            energy_supplier_id=DEFAULT_ENERGY_SUPPLIER_ID,
            balance_responsible_id=DEFAULT_BALANCE_RESPONSIBLE_ID,
        )
    ]
    result_df = _create_result_df(spark, row)
    batch_id = str(uuid.uuid4())
    sut = ProcessStepResultWriter(
        str(tmpdir),
        batch_id,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )

    # Act
    sut.write(
        result_df,
        TimeSeriesType.PRODUCTION,
        aggregation_level,
    )

    # Assert
    actual_df = spark.read.table(TABLE_NAME).where(
        col(ResultTableColName.batch_id) == batch_id
    )
    assert actual_df.collect()[0]["aggregation_level"] == aggregation_level.value


batch_id = str(uuid.uuid4())  # Needed in both test param and test implementation


@pytest.mark.parametrize(
    "column_name, column_value",
    [
        (ResultTableColName.batch_id, batch_id),
        (ResultTableColName.batch_execution_time_start, DEFAULT_BATCH_EXECUTION_START),
        (ResultTableColName.batch_process_type, DEFAULT_PROCESS_TYPE),
        (ResultTableColName.time_series_type, DEFAULT_TIME_SERIES_TYPE),
        (ResultTableColName.grid_area, DEFAULT_GRID_AREA),
        (ResultTableColName.out_grid_area, DEFAULT_TO_GRID_AREA),
        (ResultTableColName.balance_responsible_id, DEFAULT_BALANCE_RESPONSIBLE_ID),
        (ResultTableColName.energy_supplier_id, DEFAULT_ENERGY_SUPPLIER_ID),
        (ResultTableColName.time, datetime(2020, 1, 1, 0, 0)),
        (ResultTableColName.quantity, DEFAULT_QUANTITY),
        (ResultTableColName.quantity_quality, DEFAULT_QUALITY),
        (ResultTableColName.aggregation_level, DEFAULT_AGGREGATION_LEVEL),
    ],
)
def test__write__writes_column(
    spark: SparkSession,
    tmpdir: Path,
    column_name: str,
    column_value: Any,
    migrations_executed: None,
) -> None:
    # Arrange
    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA,
            to_grid_area=DEFAULT_TO_GRID_AREA,
            energy_supplier_id=DEFAULT_ENERGY_SUPPLIER_ID,
            balance_responsible_id=DEFAULT_BALANCE_RESPONSIBLE_ID,
            quality=DEFAULT_QUALITY,
            quantity=DEFAULT_QUANTITY,
        )
    ]
    result_df = _create_result_df(spark, row)
    batch_id = str(uuid.uuid4())
    sut = ProcessStepResultWriter(
        str(tmpdir),
        batch_id,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )

    # Act
    sut.write(
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )

    # Assert
    actual_df = spark.read.table(TABLE_NAME).where(
        col(ResultTableColName.batch_id) == batch_id
    )
    assert actual_df.collect()[0][column_name] == column_value
