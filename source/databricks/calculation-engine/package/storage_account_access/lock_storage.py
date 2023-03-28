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

import sys
from package.storage_account_access.data_lake_file_manager import (
    DataLakeFileManagerFactory,
)
from package import log
import configargparse
from configargparse import argparse

_LOCK_FILE_NAME = "DATALAKE_IS_LOCKED"


def _get_valid_args_or_throw(command_line_args: list[str]) -> argparse.Namespace:
    p = configargparse.ArgParser(
        description="Locks/unlocks the storage of the specified container",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    args, unknown_args = p.parse_known_args(command_line_args)
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args


def _lock() -> None:
    file_manager = DataLakeFileManagerFactory.create_instance()
    file_manager.create_file(_LOCK_FILE_NAME)
    log(f"created lock file: {_LOCK_FILE_NAME}")


def _unlock() -> None:
    file_manager = DataLakeFileManagerFactory.create_instance()
    file_manager.delete_file(_LOCK_FILE_NAME)
    log(f"deleted lock file: {_LOCK_FILE_NAME}")


def islocked() -> bool:
    file_manager = DataLakeFileManagerFactory.create_instance()
    return file_manager.exists_file(_LOCK_FILE_NAME)


# This method must remain parameterless because it will be called from the entry point when deployed.
def lock() -> None:
    _get_valid_args_or_throw(sys.argv[1:])
    _lock()


# This method must remain parameterless because it will be called from the entry point when deployed.
def unlock() -> None:
    _get_valid_args_or_throw(sys.argv[1:])
    _unlock()
