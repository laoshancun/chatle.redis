// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public interface IRedisStore
    {
        /// <summary>
        ///     Returns true just after the Store has been created, false thereafter
        /// </summary>
        /// <returns>
        ///     true if the Store has just been created, false otherwise
        /// </returns>
        bool EnsureCreated([NotNull] IModel model);

        bool FlushDatabase();

		Task<bool> FlushDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken));

		IEnumerable<object[]> GetResultsEnumerable([NotNull] RedisQuery entityType);

		IAsyncEnumerable<object[]> GetResultsAsyncEnumerable([NotNull] RedisQuery redisQuery);

		int ExecuteTransaction([NotNull] IEnumerable<IUpdateEntry> entries);

		Task<int> ExecuteTransactionAsync([NotNull] IEnumerable<IUpdateEntry> entries, CancellationToken cancellationToken = default(CancellationToken));
	}
}
