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
import subprocess
import pytest
from pyspark.sql.functions import col
from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
    IntegerType,
    LongType,
)
from tests.contract_utils import assert_contract_matches_schema
from package.calculator_job import start as start_calculator


def _get_process_manager_parameters(filename):
    """Get the parameters as they are expected to be received from the process manager."""
    with open(filename) as file:
        text = file.read()
        text = text.replace("{batch-id}", "any-guid-id")
        lines = text.splitlines()
        return list(
            filter(lambda line: not line.startswith("#") and len(line) > 0, lines)
        )


def test_calculator_job_when_invoked_with_incorrect_parameters_fails(
    integration_tests_path, databricks_path
):
    exit_code = subprocess.call(
        [
            "python",
            f"{databricks_path}/package/calculator_job.py",
            "--unexpected-arg",
        ]
    )

    assert (
        exit_code != 0
    ), "Calculator job should return non-zero exit when invoked with bad arguments"


def test_calculator_job_accepts_parameters_from_process_manager(
    data_lake_path, source_path, databricks_path
):
    """
    This test works in tandem with a .NET test ensuring that the calculator job accepts
    the arguments that are provided by the calling process manager.
    """

    # Arrange
    process_manager_parameters = _get_process_manager_parameters(
        f"{source_path}/contracts/calculation-job-parameters-reference.txt"
    )

    python_parameters = [
        "python",
        f"{databricks_path}/package/calculator_job.py",
        "--data-storage-account-name",
        "foo",
        "--data-storage-account-key",
        "foo",
        "--integration-events-path",
        "foo",
        "--time-series-points-path",
        "foo",
        "--process-results-path",
        "foo",
        "--only-validate-args",
        "1",
        "--time-zone",
        "Europe/Copenhagen",
    ]
    python_parameters.extend(process_manager_parameters)

    # Act
    exit_code = subprocess.call(python_parameters)

    # Assert
    assert exit_code == 0, "Calculator job failed to accept provided input arguments"


def test_calculator_job_input_and_output_integration_test(
    spark,
    json_test_files,
    databricks_path,
    data_lake_path,
    source_path,
    find_first_file,
):
    """This a massive test that tests multiple aspects of the job.
    It ain't pretty but most of the aspects need to be tested in conjunction
    with a successful execution in order to be (relatively) sure
    that they provide the desired guarantees.

    We haven't split this test into multiple tests, which would have to all
    run the very slow process of starting spark instance over again
    each time the external process is executed.
    """

    # Reads integration_events json file into dataframe and writes it to parquet
    spark.read.json(f"{json_test_files}/integration_events.json").withColumn(
        "body", col("body").cast("binary")
    ).write.mode("overwrite").parquet(
        f"{data_lake_path}/parquet_test_files/integration_events"
    )

    # Schema should match published_time_series_points_schema in time series
    published_time_series_points_schema = StructType(
        [
            StructField("GsrnNumber", StringType(), True),
            StructField("TransactionId", StringType(), True),
            StructField("Quantity", DecimalType(18, 3), True),
            StructField("Quality", LongType(), True),
            StructField("Resolution", LongType(), True),
            StructField("RegistrationDateTime", TimestampType(), True),
            StructField("storedTime", TimestampType(), False),
            StructField("time", TimestampType(), True),
            StructField("year", IntegerType(), True),
            StructField("month", IntegerType(), True),
            StructField("day", IntegerType(), True),
        ]
    )

    # Reads time_series_points json file into dataframe with published_time_series_points_schema and writes it to parquet
    spark.read.schema(published_time_series_points_schema).json(
        f"{json_test_files}/time_series_points.json"
    ).write.mode("overwrite").parquet(
        f"{data_lake_path}/parquet_test_files/time_series_points"
    )

    # Arrange
    python_parameters = [
        "python",
        f"{databricks_path}/package/calculator_job.py",
        "--data-storage-account-name",
        "foo",
        "--data-storage-account-key",
        "foo",
        "--integration-events-path",
        f"{data_lake_path}/parquet_test_files/integration_events",
        "--time-series-points-path",
        f"{data_lake_path}/parquet_test_files/time_series_points",
        "--process-results-path",
        f"{data_lake_path}/results",
        "--batch-id",
        "1",
        "--batch-grid-areas",
        "[805, 806]",
        "--batch-snapshot-datetime",
        "2022-09-02T21:59:00Z",
        "--batch-period-start-datetime",
        "2022-04-01T22:00:00Z",
        "--batch-period-end-datetime",
        "2022-09-01T22:00:00Z",
        "--time-zone",
        "Europe/Copenhagen",
    ]

    # Reads time_series_points from parquet into dataframe
    input_time_series_points = spark.read.parquet(
        f"{data_lake_path}/parquet_test_files/time_series_points"
    )

    # Act
    exitcode = subprocess.call(python_parameters)

    # Assert
    assert exitcode == 0
    result_805 = spark.read.json(f"{data_lake_path}/results/batch_id=1/grid_area=805")
    result_806 = spark.read.json(f"{data_lake_path}/results/batch_id=1/grid_area=806")
    assert result_805.count() >= 1, "Calculator job failed to write files"
    assert result_806.count() >= 1, "Calculator job failed to write files"

    # Asserts that the published-time-series-points contract matches the schema from input_time_series_points.
    # When asserting both that the calculator creates output and it does it with input data that matches
    # the time series points contract from the time-series domain (in the same test), then we can infer that the
    # calculator works with the format of the data published from the time-series domain.
    assert_contract_matches_schema(
        f"{source_path}/contracts/events/published-time-series-points.json",
        input_time_series_points.schema,
    )

    # Assert: Calculator result schema must match contract with .NET
    assert_contract_matches_schema(
        f"{source_path}/contracts/calculator-result.json",
        result_805.schema,
    )

    # Assert: Quantity output is a string encoded decimal with precision 3 (number of digits after delimiter)
    # Note that any change or violation may impact consumers that expects exactly this precision from the result
    import re

    assert re.search(r"^\d+\.\d{3}$", result_805.first().quantity)

    # Assert: Relative path of result file must match expectation of .NET
    # IMPORTANT: If the expected result path changes it probably requires .NET changes too
    expected_result_path = f"{data_lake_path}/results/batch_id=1/grid_area=805"
    actual_result_file = find_first_file(expected_result_path, "part-*.json")
    assert actual_result_file is not None


def test__creates_hour_csv_with_expected_columns_names(
    spark, test_data, test_data_job_parameters, databricks_path, data_lake_path
):
    # Act
    start_calculator(spark, test_data_job_parameters)

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/results/basis-data/batch_id=1/time-series-hour/grid_area=805"
    )

    assert actual.columns == [
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *[f"ENERGYQUANTITY{i+1}" for i in range(24)],
    ]


def test__creates_quarter_csv_with_expected_columns_names(
    spark, test_data, test_data_job_parameters, databricks_path, data_lake_path
):
    # Act
    start_calculator(spark, test_data_job_parameters)

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/results/basis-data/batch_id=1/time-series-quarter/grid_area=805"
    )

    assert actual.columns == [
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *[f"ENERGYQUANTITY{i+1}" for i in range(96)],
    ]


def test__creates_csv_per_resolution_per_grid_area(
    spark, test_data, test_data_job_parameters, databricks_path, data_lake_path
):
    # Act
    start_calculator(spark, test_data_job_parameters)

    # Assert
    basis_data_805 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/results/basis-data/batch_id=1/time-series-quarter/grid_area=805"
    )

    basis_data_806 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/results/basis-data/batch_id=1/time-series-quarter/grid_area=806"
    )

    assert (
        basis_data_805.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 805"

    assert (
        basis_data_806.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 806"
