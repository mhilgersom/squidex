﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Assets;

internal sealed class AssetsResultGraphType : SharedObjectGraphType<IResultList<EnrichedAsset>>
{
    public AssetsResultGraphType(IGraphType assetsList)
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = "AssetResultDto";

        AddField(new FieldType
        {
            Name = "total",
            ResolvedType = Scalars.NonNullInt,
            Resolver = ResolveList(x => x.Total),
            Description = FieldDescriptions.AssetsTotal,
        });

        AddField(new FieldType
        {
            Name = "items",
            ResolvedType = new NonNullGraphType(assetsList),
            Resolver = ResolveList(x => x),
            Description = FieldDescriptions.AssetsItems,
        });

        Description = "List of assets and total count of assets.";
    }

    private static IFieldResolver ResolveList<T>(Func<IResultList<EnrichedAsset>, T> resolver)
    {
        return Resolvers.Sync(resolver);
    }
}
