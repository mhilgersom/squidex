﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reactive.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Infrastructure;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public class GraphQLSubscriptionTests : GraphQLTestBase
{
    [Fact]
    public async Task Should_subscribe_to_assets()
    {
        var id = DomainId.NewGuid();

        var stream =
            Observable.Return<object>(
                new EnrichedAssetEvent
                {
                    Id = id,
                    FileName = "image.png",
                    FileSize = 1024,
                });

        A.CallTo(() => subscriptionService.SubscribeAsync($"asset-{TestApp.Default.Id}", A<AssetSubscription>._, default))
            .Returns(stream);

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                subscription {
                  assetChanges {
                    id,
                    fileName,
                    fileSize
                  }
                }",
            Permissions = [PermissionIds.AppAssetsRead],
        });

        var expected = new
        {
            data = new
            {
                assetChanges = new
                {
                    id,
                    fileName = "image.png",
                    fileSize = 1024,
                },
            },
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_return_error_if_user_has_no_permissions_for_assets()
    {
        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                subscription {
                  assetChanges {
                    id,
                    fileName,
                    fileSize
                  }
                }",
        });

        var expected = new
        {
            errors = new[]
            {
                new
                {
                    message = "You do not have the necessary permission.",
                    locations = new[]
                    {
                        new
                        {
                            line = 3,
                            column = 19,
                        },
                    },
                    path = new[]
                    {
                        "assetChanges",
                    },
                    extensions = new
                    {
                        code = "DOMAIN_FORBIDDEN",
                        codes = new[]
                        {
                            "DOMAIN_FORBIDDEN",
                        },
                    },
                },
            },
            data = (object?)null,
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_subscribe_to_contents()
    {
        var id = DomainId.NewGuid();

        var stream =
            Observable.Return<object>(
                new EnrichedContentEvent
                {
                    Id = id,
                    Data = new ContentData()
                        .AddField("field",
                            new ContentFieldData()
                                .AddInvariant(42)),
                });

        A.CallTo(() => subscriptionService.SubscribeAsync($"content-{TestApp.Default.Id}", A<ContentSubscription>._, default))
            .Returns(stream);

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                subscription {
                  contentChanges {
                    id,
                    data
                  }
                }",
            Permissions = [PermissionIds.AppContentsRead],
        });

        var expected = new
        {
            data = new
            {
                contentChanges = new
                {
                    id,
                    data = new
                    {
                        field = new
                        {
                            iv = 42,
                        },
                    },
                },
            },
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_return_error_if_user_has_no_permissions_for_contents()
    {
        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                subscription {
                  contentChanges {
                    id,
                    data
                  }
                }",
        });

        var expected = new
        {
            errors = new[]
            {
                new
                {
                    message = "You do not have the necessary permission.",
                    locations = new[]
                    {
                        new
                        {
                            line = 3,
                            column = 19,
                        },
                    },
                    path = new[]
                    {
                        "contentChanges",
                    },
                    extensions = new
                    {
                        code = "DOMAIN_FORBIDDEN",
                        codes = new[]
                        {
                            "DOMAIN_FORBIDDEN",
                        },
                    },
                },
            },
            data = (object?)null,
        };

        AssertResult(expected, actual);
    }
}
