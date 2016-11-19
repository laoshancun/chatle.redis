// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Redis.Tests
{
    public class RedisOptionsExtensionTest
    {
        private static readonly MethodInfo _applyServices
            = typeof(RedisOptionsExtension).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "ApplyServices");

        [Fact]
        public void Adds_redis_services()
        {
            var services = new ServiceCollection();
            
            _applyServices.Invoke(new RedisOptionsExtension(), new object[] { services });

            Assert.True(services.Any(sd => sd.ServiceType == typeof(IRedisDatabase)));
        }
    }
}
