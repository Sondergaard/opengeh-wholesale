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


class Colname:
    added_grid_loss = "added_grid_loss"
    added_system_correction = "added_system_correction"
    aggregated_quality = "aggregated_quality"
    balance_responsible_id = "balance_responsible_id"
    charge_count = "charge_count"
    charge_id = "charge_id"
    charge_key = "charge_key"
    charge_owner = "charge_owner"
    charge_price = "charge_price"
    charge_tax = "charge_tax"
    charge_type = "charge_type"
    connection_state = "connection_state"
    currency = "currency"
    date = "date"
    day = "day"
    """Can represent different things like registration day, creation day, receival day."""
    effective_date = "effective_date"
    end = "end"
    end_datetime = "end_datetime"
    energy_supplier_id = "energy_supplier_id"
    from_date = "from_date"
    grid_area = "grid_area"
    grid_loss = "grid_loss"
    gsrn_number = "gsrn_number"
    in_grid_area = "in_grid_area"
    is_grid_loss = "is_grid_loss"
    is_system_correction = "is_system_correction"
    job_id = "job_id"
    metering_method = "metering_method"
    metering_point_id = "metering_point_id"
    metering_point_type = "Type"
    month = "month"
    """Can represent different things like registration month, creation month, receival month."""
    net_settlement_group = "net_settlement_group"
    out_grid_area = "out_grid_area"
    parent_metering_point_id = "parent_metering_point_id"
    price_per_day = "price_per_day"
    product = "product"
    quality = "quality"
    quantity = "quantity"
    registration_date_time = "registration_date_time"
    resolution = "resolution"
    result_id = "result_id"
    result_name = "result_name"
    result_path = "result_path"
    settlement_method = "settlement_method"
    snapshot_id = "snapshot_id"
    start = "start"
    start_datetime = "start_datetime"
    registration_date_time = "registration_date_time"
    """When the (individual) time series was received by the system."""
    sum_quantity = "sum_quantity"
    time = "time"
    """When the production/consumption/exchange actually happened."""
    timeseries = "timeseries"
    time_window = "time_window"
    time_window_end = "time_window.end"
    time_window_start = "time_window.start"
    to_date = "to_date"
    total_daily_charge_price = "total_daily_charge_price"
    total_amount = "total_amount"
    total_quantity = "total_quantity"
    transaction_id = "transaction_id"
    unit = "unit"
    year = "year"
    """Can represent different things like registration year, creation year, receival year."""
