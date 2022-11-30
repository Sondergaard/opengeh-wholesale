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

"""
This test is commented out as it is not needed anymore. but it is keept here until we have transisioned to using static datasources
from datetime import datetime, timedelta
import pytest
from package.balance_fixing_total_production import _get_grid_areas_df
from package.schemas import grid_area_updated_event_schema
from pyspark.sql.functions import col, struct, to_json, from_json
from tests.contract_utils import (
    assert_contract_matches_schema,
    read_contract,
    get_message_type,
)


# Factory defaults
grid_area_code = "805"
grid_area_link_id = "the-grid-area-link-id"
message_type = "GridAreaUpdated"  # The exact name of the event of interest

# Beginning of the danish date (CEST)
first_of_june = datetime.strptime("31/05/2022 22:00", "%d/%m/%Y %H:%M")
second_of_june = first_of_june + timedelta(days=1)
third_of_june = first_of_june + timedelta(days=2)


@pytest.fixture
def grid_area_df_factory(spark):
    def factory(
        stored_time=first_of_june,
        grid_area_code=grid_area_code,
        grid_area_link_id=grid_area_link_id,
        message_type=message_type,
        operation_time=first_of_june,
    ):
        row = [
            {
                "storedTime": stored_time,
                "OperationTime": operation_time,
                "GridAreaCode": grid_area_code,
                "GridAreaLinkId": grid_area_link_id,
                "MessageType": message_type,
            }
        ]

        return (
            spark.createDataFrame(row)
            .withColumn(
                "body",
                to_json(
                    struct(
                        col("GridAreaCode"),
                        col("GridAreaLinkId"),
                        col("MessageType"),
                        col("OperationTime"),
                    )
                ),
            )
            .select("storedTime", "body")
        )

    return factory


@pytest.fixture
def batch_grid_area_df_factory(spark):
    def factory(grid_area_code=grid_area_code):
        row = {
            "GridAreaCode": grid_area_code,
        }
        return spark.createDataFrame([row])

    return factory


def test__stored_time_matches_persister(grid_area_df_factory, source_path):
    # Test that the anticipated stored time column name matches the column that was created
    # by the integration events persister. This test uses the shared contract.
    cached_integration_events_df = grid_area_df_factory()

    expected_stored_time_name = read_contract(
        f"{source_path}/contracts/market-participant-domain/grid-area-updated.json"
    )["storedTimeName"]

    assert expected_stored_time_name in cached_integration_events_df.columns


def test__when_input_data_matches_contract__returns_expected_row(
    grid_area_df_factory, source_path, batch_grid_area_df_factory
):
    cached_integration_events_df = grid_area_df_factory()

    # Assert: Contract matches schema
    assert_contract_matches_schema(
        f"{source_path}/contracts/market-participant-domain/grid-area-updated.json",
        grid_area_updated_event_schema,
    )
    test_data_schema = (
        cached_integration_events_df.select(col("body").cast("string"))
        .withColumn("body", from_json(col("body"), grid_area_updated_event_schema))
        .select(col("body.*"))
        .schema
    )

    # Assert: Test data schema matches schema
    assert test_data_schema == grid_area_updated_event_schema

    # Assert: From previous asserts:
    # If schema matches contract and test data matches schema and test data results in
    # the expected row we know that the production code works correct with data that complies with the contract
    actual_df = _get_grid_areas_df(
        cached_integration_events_df, batch_grid_area_df_factory()
    )
    assert actual_df.count() == 1


def test__when_using_same_message_type_as_ingestor__returns_correct_grid_area_data(
    grid_area_df_factory, source_path, batch_grid_area_df_factory
):
    # Arrange
    message_type = get_message_type(
        f"{source_path}/contracts/market-participant-domain/grid-area-updated.json"
    )
    cached_integration_events_df = grid_area_df_factory(message_type=message_type)

    # Act
    actual_df = _get_grid_areas_df(
        cached_integration_events_df, batch_grid_area_df_factory()
    )

    # Assert
    assert actual_df.count() == 1


def test__returns_correct_grid_area_data(
    grid_area_df_factory, batch_grid_area_df_factory
):
    # Arrange
    cached_integration_events_df = grid_area_df_factory()

    # Act
    actual_df = _get_grid_areas_df(
        cached_integration_events_df, batch_grid_area_df_factory()
    )

    # Assert
    actual = actual_df.first()
    assert actual.GridAreaCode == grid_area_code
    assert actual.GridAreaLinkId == grid_area_link_id


def test__when_grid_area_code_does_not_match__throws_because_grid_area_not_found(
    grid_area_df_factory, batch_grid_area_df_factory
):
    # Arrange
    non_matching_grid_area_code = "999"
    cached_integration_events_df = grid_area_df_factory(
        stored_time=first_of_june, grid_area_code=grid_area_code
    )

    # Act and assert exception
    with pytest.raises(Exception, match=r".* grid areas .*"):
        _get_grid_areas_df(
            cached_integration_events_df,
            batch_grid_area_df_factory(grid_area_code=non_matching_grid_area_code),
        )


def test__when_message_type_is_not_grid_area_updated__throws_because_grid_area_not_found(
    grid_area_df_factory, batch_grid_area_df_factory
):
    # Arrange
    cached_integration_events_df = grid_area_df_factory(
        message_type="some-other-message-type"
    )

    # Act and assert exception
    with pytest.raises(Exception, match=r".* grid areas .*"):
        _get_grid_areas_df(cached_integration_events_df, batch_grid_area_df_factory())


def test__returns_newest_grid_area_state(
    grid_area_df_factory, batch_grid_area_df_factory
):
    # Arrange
    expected_grid_area_link_id = "foo"
    unexpected_grid_area_link_id = "bar"

    # Make the row of interest the middle to increase the likeliness of the succes
    # to not depend on the row being selected by incident by being e.g. the first or last row.
    cached_integration_events_df = (
        grid_area_df_factory(
            operation_time=first_of_june,
            grid_area_link_id=unexpected_grid_area_link_id,
        )
        .union(
            grid_area_df_factory(
                operation_time=second_of_june,
                grid_area_link_id=expected_grid_area_link_id,
            )
        )
        .union(
            grid_area_df_factory(
                operation_time=third_of_june,
                grid_area_link_id=expected_grid_area_link_id,
            )
        )
    ).withColumn("body", col("body").cast("string"))

    # Act
    actual_df = _get_grid_areas_df(
        cached_integration_events_df, batch_grid_area_df_factory()
    )

    # Assert
    assert actual_df.count() == 1
    actual = actual_df.first()
    assert actual.GridAreaLinkId == expected_grid_area_link_id


def test__duplicate_grid_area_events_does_not_affect_amount_of_grid_areas(
    grid_area_df_factory, batch_grid_area_df_factory
):
    # Arrange
    cached_integration_events_df = grid_area_df_factory()

    # Duplicate 'gridAreaUpdated' events
    integration_events_df_with_dublicates = cached_integration_events_df.union(
        cached_integration_events_df
    )

    # Act
    actual_df = _get_grid_areas_df(
        integration_events_df_with_dublicates, batch_grid_area_df_factory()
    )

    # Assert
    assert actual_df.count() == 1
"""
