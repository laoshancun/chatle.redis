// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Chatle.EntityFrameworkCore.Redis.FunctionalTests;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.EntityFrameworkCore.Storage;

namespace Chatle.EntityFrameworkCore.Redis.Tests
{
    public class RedisServiceCollectionExtensionsTest : EntityFrameworkServiceCollectionExtensionsTest
    {
        [Fact]
        public void Calling_AddEntityFramework_explicitly_does_not_change_services()
            => AssertServicesSame(
                new ServiceCollection().AddEntityFrameworkRedisDatabase(),
                new ServiceCollection().AddEntityFramework().AddEntityFrameworkRedisDatabase());

        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            // Redis singletones
            VerifySingleton<RedisValueGeneratorCache>();
            VerifySingleton<RedisModelSource>();
            VerifySingleton<IRedisConnectionMultiplexerFactory>();
            VerifySingleton<IRedisTableFactory>();

            // Redis scoped
            VerifyScoped<IRedisConnection>();
            VerifyScoped<IRedisStore>();
            VerifyScoped<RedisValueGeneratorSelector>();
            VerifyScoped<RedisDatabaseProviderServices>();
            VerifyScoped<IRedisDatabase>();
            VerifyScoped<RedisTransactionManager>();
            VerifyScoped<RedisDatabaseCreator>();
        }

        public RedisServiceCollectionExtensionsTest()
            : base(RedisTestHelpers.Instance)
        {
        }
    }
}
