// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore
{
    public static class RedisDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder<TContext> UseRedisDatabase<TContext>([NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [CanBeNull] string hostName = "127.0.0.1",
            [CanBeNull] int port = 6379, 
            [CanBeNull] int database = 0, 
            [CanBeNull] int connectTimeout = 1000, 
            [CanBeNull] int syncTimeout = 1000,
            [CanBeNull] bool ignoreTransactions = true)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseRedisDatabase(
                    (DbContextOptionsBuilder)optionsBuilder, hostName, port, database, connectTimeout, syncTimeout, ignoreTransactions);

        public static DbContextOptionsBuilder<TContext> UseRedisDatabase<TContext>([NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] RedisOptionsExtension options)
            where TContext : DbContext
                => (DbContextOptionsBuilder<TContext>)UseRedisDatabase(
                    (DbContextOptionsBuilder)optionsBuilder, options);


        public static DbContextOptionsBuilder UseRedisDatabase([NotNull] this DbContextOptionsBuilder optionsBuilder,
			[CanBeNull] string hostName = "127.0.0.1",
            [CanBeNull] int port = 6379, 
            [CanBeNull] int database = 0,
            [CanBeNull] int connectTimeout = 1000, 
            [CanBeNull] int syncTimeout = 1000, 
            [CanBeNull] bool ignoreTransactions = true)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            return optionsBuilder.UseRedisDatabase(new RedisOptionsExtension()
			{
				HostName = hostName,
				Port = port,
				Database = database,
				ConnectTimeout = connectTimeout,
				SyncTimeout = syncTimeout,
                IgnoreTransactions = ignoreTransactions
			});
        }

        public static DbContextOptionsBuilder UseRedisDatabase([NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] RedisOptionsExtension options)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(options);

            return optionsBuilder;
        }
    }
}
