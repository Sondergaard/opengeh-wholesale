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

from .data_lake_file_manager import DataLakeFileManager
from package import log
from package.infrastructure import WHOLESALE_CONTAINER_NAME

_LOCK_FILE_NAME = "DATALAKE_IS_LOCKED"


def _lock(args: argparse.Namespace) -> None:

    file_manager = DataLakeFileManager(
        args.data_storage_account_name,
        args.data_storage_account_key,
        WHOLESALE_CONTAINER_NAME,
    )

    file_manager.create_file(_LOCK_FILE_NAME)
    log(f"created lock file: {_LOCK_FILE_NAME}")


def _unlock(args: argparse.Namespace) -> None:
    file_manager = DataLakeFileManager(
        args.data_storage_account_name,
        args.data_storage_account_key,
        WHOLESALE_CONTAINER_NAME,
    )
    file_manager.delete_file(_LOCK_FILE_NAME)
    log(f"deleted lock file: {_LOCK_FILE_NAME}")


def islocked(storage_account_name: str, storage_account_key: str) -> bool:
    file_manager = DataLakeFileManager(
        storage_account_name,
        storage_account_key,
        WHOLESALE_CONTAINER_NAME,
    )
    return file_manager.exists_file(_LOCK_FILE_NAME)


# This method must remain parameterless because it will be called from the entry point when deployed.
def lock() -> None:
    _lock(args)


# This method must remain parameterless because it will be called from the entry point when deployed.
def unlock() -> None:
    _unlock(args)
