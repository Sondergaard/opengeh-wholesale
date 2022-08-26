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
from datetime import datetime, timedelta
from package.balance_fixing_total_production import (
    _get_time_series_points,
)
from tests.contract_utils import (
    assert_contract_matches_schema,
    read_contract,
    get_message_type,
)

first_of_june = datetime.strptime("31/05/2022 22:00", "%d/%m/%Y %H:%M")
second_of_june = first_of_june + timedelta(days=1)
third_of_june = first_of_june + timedelta(days=2)


@pytest.fixture(scope="module")
def raw_time_series_points_factory(spark, timestamp_factory):
    def factory(
        storedTime: datetime = timestamp_factory("2022-06-10T12:09:15.000Z"),
    ):
        df = [
            {
                "GsrnNumber": "the-gsrn-number",
                "TransactionId": "1",
                "Quantity": Decimal("1.1"),
                "Quality": 3,
                "Resolution": 2,
                "RegistrationDateTime": timestamp_factory("2022-06-10T12:09:15.000Z"),
                "storedTime": storedTime,
                "time": timestamp_factory("2022-06-08T12:09:15.000Z"),
                "year": 2022,
                "month": 6,
                "day": 10,
            }
        ]
        return spark.createDataFrame(df)

    return factory


def test__raw_time_series_points_with_stored_time_of_after_snapshot_time_is_not_included_in_time_series_points_df(
    raw_time_series_points_factory,
):
    # Arrange
    time_series_points_first_of_june = raw_time_series_points_factory(first_of_june)
    time_series_points_second_of_june = raw_time_series_points_factory(second_of_june)
    time_series_points_third_of_june = raw_time_series_points_factory(third_of_june)

    time_series_points = time_series_points_first_of_june.union(
        time_series_points_second_of_june
    ).union(time_series_points_third_of_june)

    assert _get_time_series_points(time_series_points, first_of_june).count() == 1
    assert _get_time_series_points(time_series_points, second_of_june).count() == 2
    assert _get_time_series_points(time_series_points, third_of_june).count() == 3
