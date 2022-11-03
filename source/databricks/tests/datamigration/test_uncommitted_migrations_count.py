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
import subprocess
import unittest
from unittest.mock import patch, Mock, MagicMock

from package.datamigration.uncommitted_migrations_count import (
    _download_committed_migrations,
    _get_all_migrations,
    _get_uncommitted_migrations_count,
)


def test__uncommitted_migrations_count__when_invoked_with_incorrect_parameters__fails(
    databricks_path,
):
    # Act
    exit_code = subprocess.call(
        [
            "python",
            f"{databricks_path}/package/datamigration/uncommitted_migrations_count.py",
            "--unexpected-arg",
        ]
    )

    # Assert
    assert (
        exit_code != 0
    ), "Expected to return non-zero exit code when invoked with bad arguments"


def test__uncommitted_migrations_count__when_invoked_with_correct_parameters__succeeds(
    databricks_path,
):
    # Arrange
    python_parameters = [
        "python",
        f"{databricks_path}/package/datamigration/uncommitted_migrations_count.py",
        "--data-storage-account-name",
        "foo",
        "--data-storage-account-key",
        "foo",
        "--wholesale-container-name",
        "foo",
        "--only-validate-args",
        "1",
    ]

    # Act
    exit_code = subprocess.call(python_parameters)

    # Assert
    assert exit_code == 0, "Failed to accept provided input arguments"


@patch("package.datamigration.uncommitted_migrations_count.download_csv")
def test__download_committed_migrations__returns_correct_items(
    mock_download_csv,
):
    # Arrange
    migration_name_1 = "my_migration1"
    migration_name_2 = "my_migration2"
    mock_download_csv.return_value = [
        [migration_name_1],
        [migration_name_2],
    ]

    # Act
    migrations = _download_committed_migrations("", "", "")

    # Assert
    assert migrations[0] == migration_name_1 and migrations[1] == migration_name_2


@patch("package.datamigration.uncommitted_migrations_count.download_csv")
def test__download_committed_migrations__when_empty_file__returns_empty_list(
    mock_download_csv,
):
    # Arrange
    mock_download_csv.return_value = []

    # Act
    migrations = _download_committed_migrations("", "", "")

    # Assert
    assert len(migrations) == 0


@patch("package.datamigration.uncommitted_migrations_count._get_all_migrations")
@patch(
    "package.datamigration.uncommitted_migrations_count._download_committed_migrations"
)
def test__get_uncommitted_migrations_count__when_no_migration_needed__returns_0(
    mock_download_committed_migrations, mock_get_all_migrations
):
    # Arrange
    migration_name_1 = "my_migration1"
    migration_name_2 = "my_migration2"
    mock_download_committed_migrations.return_value = [
        migration_name_1,
        migration_name_2,
    ]
    mock_get_all_migrations.return_value = [migration_name_1, migration_name_2]

    # Act
    count = _get_uncommitted_migrations_count("", "", "")

    # Assert
    assert count == 0


@patch("package.datamigration.uncommitted_migrations_count._get_all_migrations")
@patch(
    "package.datamigration.uncommitted_migrations_count._download_committed_migrations"
)
def test__get_uncommitted_migrations_count__when_one_migration_needed__returns_1(
    mock_download_committed_migrations, mock_get_all_migrations
):
    # Arrange
    migration_name_1 = "my_migration1"
    migration_name_2 = "my_migration2"
    mock_download_committed_migrations.return_value = [migration_name_1]
    mock_get_all_migrations.return_value = [
        migration_name_1,
        migration_name_2,
    ]

    # Act
    count = _get_uncommitted_migrations_count("", "", "")

    # Assert
    assert count == 1
