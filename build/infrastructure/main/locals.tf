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

locals {
    PROCESSES_CONTAINER_NAME                                 = "processes"

    # Batch completed events
    COMPLETED_BATCH_SUBSCRIPTION_ZIP_BASIS_DATA              = "completed-batch-sub-zip-basis-data"
    COMPLETED_BATCH_SUBSCRIPTION_PUBLISH_PROCESSES_COMPLETED = "completed-batch-sub-publish-processes-completed"
    
    # Process completed
    COMPLETED_PROCESS_SUBSCRIPTION_SEND_DATA_AVAILABLE       = "completed-process-sub-send-data-available"
    
    # Integration events
    INTERGRATION_EVENTS_CONTAINER_NAME                       = "integration-events"

    # Database
    DB_CONNECTION_STRING_SQL_AUTH                            = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_wholesale.name};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.mssql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.mssql_data_admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
    DB_CONNECTION_STRING                                     = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Database=${module.mssqldb_wholesale.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"

    TIME_ZONE                                                = "Europe/Copenhagen"

    # Domain event names
    BATCH_COMPLETED_EVENT_NAME                               = "BatchCompleted"
    PROCESS_COMPLETED_EVENT_NAME                             = "ProcessCompleted"
}
