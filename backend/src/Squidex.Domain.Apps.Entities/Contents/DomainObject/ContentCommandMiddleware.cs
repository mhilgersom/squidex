﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject;

public sealed class ContentCommandMiddleware(
    IDomainObjectFactory domainObjectFactory,
    IDomainObjectCache domainObjectCache,
    IContentEnricher contentEnricher,
    IContextProvider contextProvider)
    : CachingDomainObjectMiddleware<ContentCommand, ContentDomainObject, WriteContent>(domainObjectFactory, domainObjectCache)
{
    protected override async Task<object> EnrichResultAsync(CommandContext context, CommandResult result,
        CancellationToken ct)
    {
        var payload = await base.EnrichResultAsync(context, result, ct);

        if (payload is WriteContent writeContent)
        {
            payload = await contentEnricher.EnrichAsync(writeContent.ToContent(), true, contextProvider.Context, ct);
        }

        if (payload is Content content and not EnrichedContent)
        {
            payload = await contentEnricher.EnrichAsync(content, true, contextProvider.Context, ct);
        }

        return payload;
    }
}
