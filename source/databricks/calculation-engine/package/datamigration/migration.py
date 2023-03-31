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

import importlib
import sys
from azure.identity import ClientSecretCredential

import configargparse
from package import infrastructure, initialize_spark, log
from package.args_helper import valid_log_level
import package.environment_variables as env_vars
from .committed_migrations import upload_committed_migration
from package.infrastructure import WHOLESALE_CONTAINER_NAME
from package.storage_account_access.data_lake_file_manager import DataLakeFileManager
from .migration_script_args import MigrationScriptArgs
from .uncommitted_migrations import get_uncommitted_migrations
from typing import Any
from configargparse import argparse


def _apply_migration(migration_name: str, migration_args: MigrationScriptArgs) -> None:
    migration = importlib.import_module(
        "package.datamigration.migration_scripts." + migration_name
    )
    migration.apply(
        migration_args,
    )


def _migrate_data_lake(
    storage_account_name: str, storage_account_credential: ClientSecretCredential
) -> None:
    file_manager = DataLakeFileManager(
        storage_account_name, storage_account_credential, WHOLESALE_CONTAINER_NAME
    )

    spark = initialize_spark()

    uncommitted_migrations = get_uncommitted_migrations(file_manager)
    uncommitted_migrations.sort()

    storage_account_url = infrastructure.get_storage_account_url(
        storage_account_name,
    )

    migration_args = MigrationScriptArgs(
        data_storage_account_url=storage_account_url,
        data_storage_account_name=storage_account_name,
        data_storage_credential=storage_account_credential,
        spark=spark,
    )

    for name in uncommitted_migrations:
        _apply_migration(name, migration_args)
        upload_committed_migration(file_manager, name)


# This method must remain parameterless because it will be called from the entry point when deployed.
def migrate_data_lake() -> None:
    storage_account_name = env_vars.get_storage_account_name()
    credential = env_vars.get_storage_account_credential()
    _migrate_data_lake(storage_account_name, credential)
