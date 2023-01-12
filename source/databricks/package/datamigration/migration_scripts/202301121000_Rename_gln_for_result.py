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

from azure.storage.filedatalake import (
    FileSystemClient,
    DataLakeDirectoryClient,
)
from package.datamigration.migration_script_args import MigrationScriptArgs
import re


def apply(args: MigrationScriptArgs) -> None:
    container = "wholesale"
    directory_name = "calculation-output"

    # Get the file system client
    file_system_client = FileSystemClient(
        account_url=args.storage_account_url,
        file_system_name=container,
        credential=args.storage_account_key,
    )

    # Enumerate the directories in the parent folder
    directories = file_system_client.get_paths(path=directory_name)

    # Rename each directory
    for directory in directories:
        match = re.search(
            r"(calculation-output/(batch_id=\w{8}-\w{4}-\w{4}-\w{4}-\w{12}/result/grid_area=\d{3}))/gln=grid_access_provider/step=production",
            directory.name,
        )
        if match and directory.is_directory:
            base_path = match.group(1)
            current_directory_name = directory.name

            directory_client = file_system_client.get_directory_client(
                directory=current_directory_name
            )
            new_directory_name = (
                f"{base_path}/gln=grid_access_provider/time_series_type=production"
            )
            move_and_rename_folder(
                directory_client=directory_client,
                current_directory_name=current_directory_name,
                new_directory_name=new_directory_name,
                container=container,
            )

            current_directory_name_2 = f"{base_path}/gln=grid_access_provider"
            directory_client_2 = file_system_client.get_directory_client(
                directory=current_directory_name_2
            )
            new_directory_name_2 = f"{base_path}/gln=grid_area"
            move_and_rename_folder(
                directory_client=directory_client_2,
                current_directory_name=current_directory_name_2,
                new_directory_name=new_directory_name_2,
                container=container,
            )


def move_and_rename_folder(
    directory_client: DataLakeDirectoryClient,
    current_directory_name: str,
    new_directory_name: str,
    container: str,
) -> None:
    source_path = f"{container}/{current_directory_name}"
    new_path = f"{container}/{new_directory_name}"

    if not directory_client.exists():
        print(
            f"Skipping migration ({__file__}). Source directory not found:{source_path}"
        )
        return

    directory_client.rename_directory(new_name=new_path)
