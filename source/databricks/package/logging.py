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

from inspect import stack
from pyspark.sql import DataFrame


def _log(level: str, message: str, df: DataFrame):
    print(f"============ {level} ============")
    print(f"In function {stack()[1].function}:")
    print(message)
    if df is not None:
        df.printSchema()
        df.show()
    print("==============================")


def log(message: str, df: DataFrame = None):
    _log("LOG", message, df)


def debug(message: str, df: DataFrame = None):
    _log("DEBUG", message, df)
