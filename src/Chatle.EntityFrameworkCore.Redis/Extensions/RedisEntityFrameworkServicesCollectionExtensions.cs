// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisEntityFrameworkServicesBuilderExtensions
    {
        public static IServiceCollection AddEntityFrameworkRedisDatabase([NotNull] this IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));

            services.AddEntityFramework();

            services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseProvider, DatabaseProvider<RedisDatabaseProviderServices, RedisOptionsExtension>>());

			services.TryAdd(new ServiceCollection()
				.AddSingleton<RedisValueGeneratorCache>()
				.AddSingleton<RedisModelSource>()
				.AddSingleton<IRedisConnectionMultiplexerFactory, RedisConnectionMultipexerFactory>()
				.AddSingleton<IRedisTableFactory, RedisTableFactory>()
				.AddScoped<IRedisConnection, RedisConnection>()
				.AddScoped<IRedisStore, RedisStore>()
				.AddScoped<RedisValueGeneratorSelector>()
                .AddScoped<RedisDatabaseProviderServices>()
                .AddScoped<IRedisDatabase, RedisDatabase>()
                .AddScoped<RedisTransactionManager>()
                .AddScoped<RedisDatabaseCreator>()
                .AddQuery());

            return services;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
            => serviceCollection
                .AddScoped<IRedisMaterializerFactory, RedisMaterializerFactory>()
                .AddScoped<RedisQueryContextFactory>()
                .AddScoped<RedisQueryModelVisitorFactory>()
                .AddScoped<RedisEntityQueryableExpressionVisitorFactory>()
                .AddScoped<RedisQueryCompilationContextFactory>();
    }
}
