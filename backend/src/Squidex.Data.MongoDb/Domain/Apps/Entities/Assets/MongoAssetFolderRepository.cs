﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets;

public sealed partial class MongoAssetFolderRepository(IMongoDatabase database) : MongoRepositoryBase<MongoAssetFolderEntity>(database), IAssetFolderRepository
{
    static MongoAssetFolderRepository()
    {
        MongoAssetFolderEntity.RegisterClassMap();
    }

    protected override string CollectionName()
    {
        return "States_AssetFolders2";
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoAssetFolderEntity> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<MongoAssetFolderEntity>(
                Index
                    .Ascending(x => x.IndexedAppId)
                    .Ascending(x => x.ParentId)
                    .Ascending(x => x.IsDeleted)),
        ], ct);
    }

    public async Task<IResultList<AssetFolder>> QueryAsync(DomainId appId, DomainId? parentId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/QueryAsync"))
        {
            var filter = BuildFilter(appId, parentId);

            var assetFolderEntities =
                await Collection.Find(filter).SortBy(x => x.FolderName)
                    .ToListAsync(ct);

            return ResultList.Create<AssetFolder>(assetFolderEntities.Count, assetFolderEntities);
        }
    }

    public async Task<IReadOnlyList<DomainId>> QueryChildIdsAsync(DomainId appId, DomainId? parentId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/QueryChildIdsAsync"))
        {
            var filter = BuildFilter(appId, parentId);

            var assetFolderEntities =
                await Collection.Find(filter).Only(x => x.Id)
                    .ToListAsync(ct);

            var field = Field.Of<MongoAssetFolderEntity>(x => nameof(x.Id));

            return assetFolderEntities.Select(x => DomainId.Create(x[field].AsString)).ToList();
        }
    }

    public async Task<AssetFolder?> FindAssetFolderAsync(DomainId appId, DomainId id,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/FindAssetFolderAsync"))
        {
            var documentId = DomainId.Combine(appId, id);

            var assetFolderEntity =
                await Collection.Find(x => x.DocumentId == documentId && !x.IsDeleted)
                    .FirstOrDefaultAsync(ct);

            return assetFolderEntity;
        }
    }

    private static FilterDefinition<MongoAssetFolderEntity> BuildFilter(DomainId appId, DomainId? parentId)
    {
        var filters = new List<FilterDefinition<MongoAssetFolderEntity>>
        {
            Filter.Eq(x => x.IndexedAppId, appId),
            Filter.Ne(x => x.IsDeleted, true),
        };

        if (parentId != null)
        {
            if (parentId == DomainId.Empty)
            {
                filters.Add(
                    Filter.Or(
                        Filter.Exists(x => x.ParentId, false),
                        Filter.Eq(x => x.ParentId, DomainId.Empty)));
            }
            else
            {
                filters.Add(Filter.Eq(x => x.ParentId, parentId.Value));
            }
        }

        return Filter.And(filters);
    }
}
