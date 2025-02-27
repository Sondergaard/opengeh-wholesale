﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Energinet.DataHub.Wholesale.WebApi.Configuration.Options;

public class JwtOptions
{
    public string EXTERNAL_OPEN_ID_URL { get; set; } = string.Empty;

    public string INTERNAL_OPEN_ID_URL { get; set; } = string.Empty;

    /// <summary>
    /// The id of the application registration that the JWT is expected to be issued to (audience claim).
    /// Used to ensure that the received token, even if valid, is actually intended for BFF and current WebAPI.
    /// </summary>
    public string BACKEND_BFF_APP_ID { get; set; } = string.Empty;
}
