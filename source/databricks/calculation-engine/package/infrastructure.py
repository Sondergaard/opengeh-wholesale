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

# Resource names and variables defined in the infrastructure repository (https://github.com/Energinet-DataHub/dh3-infrastructure)

from typing import Union
from package.codelists import BasisDataType

WHOLESALE_CONTAINER_NAME = "wholesale"

OUTPUT_FOLDER = "calculation-output"
BASIS_DATA_FOLDER = "basis_data"


def get_storage_account_url(storage_account_name: str) -> str:
    return f"https://{storage_account_name}.dfs.core.windows.net"


def get_container_root_path(storage_account_name: str) -> str:
    return f"abfss://{WHOLESALE_CONTAINER_NAME}@{storage_account_name}.dfs.core.windows.net/"


def get_basis_data_root_path(basis_data_type: BasisDataType, batch_id: str) -> str:
    batch_path = get_batch_relative_path(batch_id)
    return f"{batch_path}/{BASIS_DATA_FOLDER}/{_get_basis_data_folder_name(basis_data_type)}"


def get_basis_data_path(
    basis_data_type: BasisDataType,
    batch_id: str,
    grid_area: str,
    energy_supplier_id: Union[str, None] = None,
) -> str:
    basis_data_root_path = get_basis_data_root_path(basis_data_type, batch_id)
    if energy_supplier_id is None:
        return f"{basis_data_root_path}/grouping=total_ga/grid_area={grid_area}"
    else:
        return f"{basis_data_root_path}/grouping=es_ga/grid_area={grid_area}/energy_supplier_gln={energy_supplier_id}"


def get_calculation_output_folder() -> str:
    return OUTPUT_FOLDER


def get_batch_relative_path(batch_id: str) -> str:
    return f"{OUTPUT_FOLDER}/batch_id={batch_id}"


def _get_basis_data_folder_name(basis_data_type: BasisDataType) -> str:
    if basis_data_type == BasisDataType.MasterBasisData:
        return "master_basis_data"
    elif basis_data_type == BasisDataType.TimeSeriesHour:
        return "time_series_hour"
    elif basis_data_type == BasisDataType.TimeSeriesQuarter:
        return "time_series_quarter"
    else:
        raise ValueError(f"Unexpected BasisDataType: {basis_data_type}")
