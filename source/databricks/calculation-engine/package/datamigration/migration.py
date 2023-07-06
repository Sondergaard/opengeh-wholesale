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
from azure.identity import ClientSecretCredential
from typing import Any

from package import infrastructure, initialize_spark, log
import package.environment_variables as env_vars
from .committed_migrations import upload_committed_migration
from package.infrastructure import WHOLESALE_CONTAINER_NAME
from package.storage_account_access.data_lake_file_manager import DataLakeFileManager
from .migration_script_args import MigrationScriptArgs
from .uncommitted_migrations import get_uncommitted_migrations
import package.datamigration.constants as c


def split_string_by_go(string: str) -> list[str]:
    lines = string.replace("\r\n", "\n").split("\n")
    sections = []
    current_section: list[str] = []

    for line in lines:
        if "go" in line.lower():
            if current_section:
                sections.append("\n".join(current_section))
                current_section = []
        else:
            current_section.append(line)

    if current_section:
        sections.append("\n".join(current_section))

    return [s for s in sections if s and not s.isspace()]


def _apply_migration(migration_name: str, migration_args: MigrationScriptArgs) -> None:
    sql_content = importlib.resources.read_text(f'{c.WHEEL_NAME}.{c.MIGRATION_SCRIPTS_FOLDER_PATH}', migration_name)
    for statement in split_string_by_go(sql_content):
        # TODO BJM: Generalize this substitute concept - must also be applied in .NET
        statement = statement.replace("{CONTAINER_PATH}", migration_args.storage_container_path)
        try:
            migration_args.spark.sql(statement)
        except Exception as e:
            raise Exception(f"Error executing SQL '{statement}. Error: {str(e)}'")


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
        data_storage_container_name=WHOLESALE_CONTAINER_NAME,
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
