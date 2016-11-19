// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class RedisQueryContext : QueryContext
    {
        public RedisQueryContext(
            [NotNull] Func<IQueryBuffer> queryBufferFactory,
            [NotNull] IRedisStore store,
            [NotNull] LazyRef<IStateManager> stateManager,
            [NotNull] IConcurrencyDetector concurrencyDetector)
            : base(
                Check.NotNull(queryBufferFactory, nameof(queryBufferFactory)),
                Check.NotNull(stateManager, nameof(stateManager)),
                Check.NotNull(concurrencyDetector, nameof(concurrencyDetector)))
        {
            Store = store;
        }

        public virtual IRedisStore Store { get; }

		public virtual IEnumerable<object[]> GetResultsEnumerable([NotNull] RedisQuery redisQuery)
		{
			Check.NotNull(redisQuery, "redisQuery");

			return Store.GetResultsEnumerable(redisQuery);
		}

		public virtual IAsyncEnumerable<object[]> GetResultsAsyncEnumerable([NotNull] RedisQuery redisQuery)
		{
			Check.NotNull(redisQuery, "redisQuery");

			return Store.GetResultsAsyncEnumerable(redisQuery);
		}

	}
}
