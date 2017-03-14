// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using StackExchange.Redis;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore
{
    public static class RedisDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder<TContext> UseRedisDatabase<TContext>([NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] RedisOptionsExtension options)
            where TContext : DbContext
                => (DbContextOptionsBuilder<TContext>)UseRedisDatabase(
                    (DbContextOptionsBuilder)optionsBuilder, options);

        public static DbContextOptionsBuilder UseRedisDatabase([NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] ConfigurationOptions options,
            [CanBeNull] bool ignoreTransactions = true)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            return optionsBuilder.UseRedisDatabase(new RedisOptionsExtension()
            {
                ConnectionOptions = options,
                IgnoreTransactions = ignoreTransactions
            });
        }
        public static DbContextOptionsBuilder UseRedisDatabase([NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string connection,
            [CanBeNull] bool ignoreTransactions = true)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            return optionsBuilder.UseRedisDatabase(new RedisOptionsExtension()
            {
                ConnectionOptions = ConfigurationOptions.Parse(connection),
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
