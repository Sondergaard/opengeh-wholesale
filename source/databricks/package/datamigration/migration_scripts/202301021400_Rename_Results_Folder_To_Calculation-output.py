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

from azure.storage.filedatalake import DataLakeDirectoryClient
from package.datamigration.migration_script_args import MigrationScriptArgs


def apply(args: MigrationScriptArgs) -> None:
    container = "wholesale"

    # Rename 'results' folder to 'calculation-output'
    current_directory_name = "results"
    new_directory_name = "calculation-output"
    rename_directory(
        args.storage_account_url,
        args.storage_account_key,
        container,
        current_directory_name,
        new_directory_name,
    )


def rename_directory(
    storage_account_url: str,
    storage_account_key: str,
    container: str,
    current_directory_name: str,
    new_directory_name: str,
) -> None:
    directory_client = DataLakeDirectoryClient(
        storage_account_url,
        container,
        current_directory_name,
        storage_account_key,
    )

    if not directory_client.exists():
        source_path = container + "/" + current_directory_name
        print(
            f"Skipping migration ({__file__}). Source directory not found:{source_path}"
        )
        return

    directory_client.rename_directory(new_name=container + "/" + new_directory_name)
