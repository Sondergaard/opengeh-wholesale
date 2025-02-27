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

using Google.Protobuf;

namespace Energinet.DataHub.Core.Messaging.Communication.Internal;

/// <summary>
/// ADR-008:  https://energinet.atlassian.net/wiki/spaces/D3/pages/328957986/ADR+008+-+Integration+events+with+protocol+buffers
/// </summary>
/// <param name="EventIdentification"></param>
/// <param name="EventName"></param>
/// <param name="EventMinorVersion"></param>
/// <param name="Message"></param>
public record IntegrationEvent(Guid EventIdentification, string EventName, int EventMinorVersion, IMessage Message);
