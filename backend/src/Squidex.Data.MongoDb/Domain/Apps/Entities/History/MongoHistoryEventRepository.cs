﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.History;

public sealed class MongoHistoryEventRepository(IMongoDatabase database) : MongoRepositoryBase<HistoryEvent>(database), IHistoryEventRepository, IDeleter
{
    static MongoHistoryEventRepository()
    {
        BsonClassMap.TryRegisterClassMap<HistoryEvent>(cm =>
        {
            cm.AutoMap();

            cm.MapProperty(x => x.OwnerId)
                .SetElementName("AppId");

            cm.MapProperty(x => x.EventType)
                .SetElementName("Message");
        });
    }

    protected override string CollectionName()
    {
        return "Projections_History";
    }

    protected override Task SetupCollectionAsync(IMongoCollection<HistoryEvent> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<HistoryEvent>(
                Index
                    .Ascending(x => x.OwnerId)
                    .Ascending(x => x.Channel)
                    .Descending(x => x.Created)
                    .Descending(x => x.Version)),
            new CreateIndexModel<HistoryEvent>(
                Index
                    .Ascending(x => x.OwnerId)
                    .Descending(x => x.Created)
                    .Descending(x => x.Version)),
        ], ct);
    }

    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        await Collection.DeleteManyAsync(Filter.Eq(x => x.OwnerId, app.Id), ct);
    }

    public async Task<IReadOnlyList<HistoryEvent>> QueryByChannelAsync(DomainId ownerId, string? channel, int count,
        CancellationToken ct = default)
    {
        var find =
            !string.IsNullOrWhiteSpace(channel) ?
                Collection.Find(x => x.OwnerId == ownerId && x.Channel == channel) :
                Collection.Find(x => x.OwnerId == ownerId);

        var result = await find.SortByDescending(x => x.Created).ThenByDescending(x => x.Version).Limit(count).ToListAsync(ct);

        return result;
    }

    public Task InsertManyAsync(IEnumerable<HistoryEvent> historyEvents,
        CancellationToken ct = default)
    {
        var writes = historyEvents
            .Select(x =>
                new ReplaceOneModel<HistoryEvent>(Filter.Eq(y => y.Id, x.Id), x)
                {
                    IsUpsert = true,
                })
            .ToList();

        if (writes.Count == 0)
        {
            return Task.CompletedTask;
        }

        return Collection.BulkWriteAsync(writes, BulkUnordered, ct);
    }
}
