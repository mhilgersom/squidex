﻿// ==========================================================================
//  MongoRuleEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Rules
{
    public sealed class MongoRuleEntity
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonElement]
        [BsonRequired]
        public int Version { get; set; }

        [BsonElement]
        [BsonRequired]
        public Guid AppId { get; set; }

        [BsonElement]
        [BsonRequired]
        public bool IsDeleted { get; set; }

        [BsonJson]
        [BsonElement]
        [BsonRequired]
        public RuleState State { get; set; }
    }
}
