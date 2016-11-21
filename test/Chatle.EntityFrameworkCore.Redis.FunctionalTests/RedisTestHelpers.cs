// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Specification.Tests;

namespace Microsoft.EntityFrameworkCore.FunctionalTests
{
    public class RedisTestHelpers : TestHelpers
    {
        protected RedisTestHelpers()
        {
        }

        public new static RedisTestHelpers Instance { get; } = new RedisTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection builder)
            => builder.AddEntityFrameworkRedisDatabase();

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseRedisDatabase();
    }
}
