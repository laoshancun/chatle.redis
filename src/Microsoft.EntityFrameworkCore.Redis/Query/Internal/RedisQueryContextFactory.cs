// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class RedisQueryContextFactory : QueryContextFactory
    {
        private readonly IRedisDatabase _database;

        public RedisQueryContextFactory(
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IConcurrencyDetector concurrencyDetector,
            [NotNull] IRedisDatabase database)
            : base(currentContext, concurrencyDetector)
        {
            Check.NotNull(database, nameof(database));

            _database = database;
        }

        public override QueryContext Create()
            => new RedisQueryContext(CreateQueryBuffer, _database.Store, StateManager, ConcurrencyDetector);
    }
}
