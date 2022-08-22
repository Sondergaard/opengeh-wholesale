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


from datetime import datetime, timedelta
import os
import shutil
import pytest
import json
from types import SimpleNamespace
from package import calculate_balance_fixing_total_production
from package.balance_fixing_total_production import _get_metering_point_periods_df
from package.schemas import (
    metering_point_created_event_schema,
    metering_point_connected_event_schema,
)
from package.codelists import (
    ConnectionState,
    MeteringPointType,
    Resolution,
    SettlementMethod,
)
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, struct, to_json, from_json
from tests.contract_utils import (
    assert_contract_matches_schema,
    read_contract,
    get_message_type,
)


# Factory defaults
grid_area_code = "805"
grid_area_link_id = "the-grid-area-link-id"
gsrn_number = "some-gsrn-number"
metering_point_id = "some-metering-point-id"
# message_type = "GridAreaUpdated"  # The exact name of the event of interest
metering_point_created_message_type = "MeteringPointCreated"
metering_point_connected_message_type = "MeteringPointConnected"

# Beginning of the danish date (CEST)
first_of_june = datetime.strptime("31/05/2022 22:00", "%d/%m/%Y %H:%M")
second_of_june = first_of_june + timedelta(days=1)
third_of_june = first_of_june + timedelta(days=2)


@pytest.fixture
def grid_area_df_factory(spark):
    def factory(
        grid_area_code=grid_area_code,
        grid_area_link_id=grid_area_link_id,
    ):
        row = [
            {
                "GridAreaCode": grid_area_code,
                "GridAreaLinkId": grid_area_link_id,
            }
        ]

        return spark.createDataFrame(row).select("GridAreaCode", "GridAreaLinkId")

    return factory


@pytest.fixture
def metering_point_created_df_factory(spark):
    def factory(
        stored_time=first_of_june,
        message_type=metering_point_created_message_type,
        operation_time=first_of_june,
        grid_area_link_id=grid_area_link_id,
        gsrn_number=gsrn_number,
        resolution=Resolution.hour,
    ):
        row = [
            {
                "storedTime": stored_time,
                "OperationTime": operation_time,
                "MessageType": message_type,
                "GsrnNumber": gsrn_number,
                "GridAreaLinkId": grid_area_link_id,
                "SettlementMethod": SettlementMethod.non_profiled,
                "ConnectionState": ConnectionState.connected,
                "EffectiveDate": first_of_june,
                "MeteringPointType": MeteringPointType.production,
                "MeteringPointId": metering_point_id,
                "Resolution": resolution,
                "CorrelationId": "some-correlation-id",
            }
        ]

        return (
            spark.createDataFrame(row)
            .withColumn(
                "body",
                to_json(
                    struct(
                        col("MessageType"),
                        col("OperationTime"),
                        col("GridAreaLinkId"),
                        col("GsrnNumber"),
                        col("GridAreaLinkId"),
                        col("MessageType"),
                        col("SettlementMethod"),
                        col("ConnectionState"),
                        col("EffectiveDate"),
                        col("MeteringPointType"),
                        col("MeteringPointId"),
                        col("Resolution"),
                        col("OperationTime"),
                        col("CorrelationId"),
                    )
                ).cast("binary"),
            )
            .select("storedTime", "body")
        )

    return factory


@pytest.fixture
def metering_point_connected_df_factory(spark):
    def factory(
        stored_time=first_of_june,
        message_type=metering_point_connected_message_type,
        operation_time=first_of_june,
        grid_area_link_id=grid_area_link_id,
        gsrn_number=gsrn_number,
        metering_point_id=metering_point_id,
    ):
        row = [
            {
                "storedTime": stored_time,
                "OperationTime": operation_time,
                "MessageType": message_type,
                "GsrnNumber": gsrn_number,
                "EffectiveDate": second_of_june,
                "MeteringPointId": metering_point_id,
                "CorrelationId": "some-other-correlation-id",
            }
        ]

        return (
            spark.createDataFrame(row)
            .withColumn(
                "body",
                to_json(
                    struct(
                        col("MessageType"),
                        col("OperationTime"),
                        col("GsrnNumber"),
                        col("MessageType"),
                        col("EffectiveDate"),
                        col("MeteringPointId"),
                        col("OperationTime"),
                        col("CorrelationId"),
                    )
                ).cast("binary"),
            )
            .select("storedTime", "body")
        )

    return factory


def test__stored_time_of_metering_point_created_matches_persister(
    metering_point_created_df_factory, source_path
):
    """Test that the anticipated stored time column name matches the column that was created
    by the integration events persister. This test uses the shared contract."""
    raw_integration_events_df = metering_point_created_df_factory()

    expected_stored_time_name = read_contract(
        f"{source_path}/contracts/events/metering-point-created.json"
    )["storedTimeName"]

    assert expected_stored_time_name in raw_integration_events_df.columns


def test__stored_time_of_metering_point_connected_matches_persister(
    metering_point_connected_df_factory, source_path
):
    """Test that the anticipated stored time column name matches the column that was created
    by the integration events persister. This test uses the shared contract."""
    raw_integration_events_df = metering_point_connected_df_factory()

    expected_stored_time_name = read_contract(
        f"{source_path}/contracts/events/metering-point-connected.json"
    )["storedTimeName"]

    assert expected_stored_time_name in raw_integration_events_df.columns


def test__when_input_data_matches_metering_point_created_contract__returns_expected_row(
    metering_point_created_df_factory, source_path, grid_area_df_factory
):
    grid_area_df = grid_area_df_factory()
    raw_integration_events_df = metering_point_created_df_factory()

    # Assert: Contract matches schema
    assert_contract_matches_schema(
        f"{source_path}/contracts/events/metering-point-created.json",
        metering_point_created_event_schema,
    )

    # Assert: Test data schema matches schema
    test_data_schema = (
        raw_integration_events_df.select(col("body").cast("string"))
        .withColumn("body", from_json(col("body"), metering_point_created_event_schema))
        .select(col("body.*"))
        .schema
    )
    assert test_data_schema == metering_point_created_event_schema

    # Assert: From previous asserts:
    # If schema matches contract and test data matches schema and test data results in
    # the expected row we know that the production code works correct with data that complies with the contract
    actual_df = _get_metering_point_periods_df(
        raw_integration_events_df,
        grid_area_df,
        first_of_june,
        second_of_june,
    )
    assert actual_df.count() == 1


def test__when_using_same_message_type_as_ingestor_for_created_event__returns_row(
    metering_point_created_df_factory, grid_area_df_factory, source_path
):
    # Arrange
    message_type = get_message_type(
        f"{source_path}/contracts/events/metering-point-created.json"
    )
    raw_integration_events_df = metering_point_created_df_factory(
        message_type=message_type
    )
    grid_area_df = grid_area_df_factory()

    # Act
    actual_df = _get_metering_point_periods_df(
        raw_integration_events_df,
        grid_area_df,
        first_of_june,
        second_of_june,
    )

    # Assert
    assert actual_df.count() == 1


def test__when_using_same_message_type_as_ingestor_for_connected_event__returns_row(
    metering_point_connected_df_factory, grid_area_df_factory, source_path
):
    # Arrange
    message_type = get_message_type(
        f"{source_path}/contracts/events/metering-point-connected.json"
    )
    raw_integration_events_df = metering_point_connected_df_factory(
        message_type=message_type
    )
    grid_area_df = grid_area_df_factory()

    # Act
    actual_df = _get_metering_point_periods_df(
        raw_integration_events_df,
        grid_area_df,
        first_of_june,
        second_of_june,
    )

    # Assert
    assert actual_df.count() == 1


def test__when_created_event__returns_correct_period(
    metering_point_created_df_factory, grid_area_df_factory
):
    # Arrange
    raw_integration_events_df = metering_point_created_df_factory()
    grid_area_df = grid_area_df_factory()

    # Act
    actual_df = _get_metering_point_periods_df(
        raw_integration_events_df,
        grid_area_df,
        first_of_june,
        second_of_june,
    )

    # Assert
    assert actual_df.count() == 1
    actual = actual_df.first()
    assert actual.MessageType == 42
    assert actual.GsrnNumber == 42
    assert actual.GridAreaCode == 42
    assert actual.EffectiveDate == 42
    assert actual.toEffectiveDate == 42
    assert actual.Resolution == 42


def test__when_connected_event__returns_correct_period(
    metering_point_connected_df_factory, grid_area_df_factory
):
    # Arrange
    raw_integration_events_df = metering_point_connected_df_factory()
    grid_area_df = grid_area_df_factory()

    # Act
    actual_df = _get_metering_point_periods_df(
        raw_integration_events_df,
        grid_area_df,
        first_of_june,
        second_of_june,
    )

    # Assert
    assert actual_df.count() == 1
    actual = actual_df.first()
    assert actual.MessageType == 42
    assert actual.GsrnNumber == 42
    assert actual.GridAreaCode == 42
    assert actual.EffectiveDate == 42
    assert actual.toEffectiveDate == 42
    assert actual.Resolution == 42
