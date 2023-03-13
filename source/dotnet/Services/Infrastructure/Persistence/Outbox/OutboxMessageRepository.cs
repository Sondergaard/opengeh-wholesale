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

using Energinet.DataHub.Wholesale.Domain;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Infrastructure.Persistence.Outbox
{
    public class OutboxMessageRepository : IOutboxMessageRepository
    {
        private readonly DatabaseContext _context;

        public OutboxMessageRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OutboxMessage message)
        {
            await _context.OutboxMessages.AddAsync(message).ConfigureAwait(false);
        }

        public async Task<IList<OutboxMessage>> GetAllAsync(CancellationToken token)
        {
            return await _context.OutboxMessages
                .Where(x => !x.ProcessedDate.HasValue)
                .OrderBy(x => x.CreationDate)
                .ToListAsync(token)
                .ConfigureAwait(false);
        }

        public void DeleteBy(Instant date)
        {
            var messagesToDelete = _context.OutboxMessages.Where(x => x.ProcessedDate.HasValue && x.ProcessedDate.Value < date);
            _context.OutboxMessages.RemoveRange(messagesToDelete);
        }
    }
}
