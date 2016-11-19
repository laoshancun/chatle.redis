// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Redis.Tests
{
    public class RedisIntegerValueGeneratorTest
    {
        [Fact]
        public void Creates_values()
        {
            var generator = new RedisIntegerValueGenerator<int>();

            Assert.Equal(1, generator.Next(null));
            Assert.Equal(2, generator.Next(null));
            Assert.Equal(3, generator.Next(null));
            Assert.Equal(4, generator.Next(null));
            Assert.Equal(5, generator.Next(null));
            Assert.Equal(6, generator.Next(null));

            generator = new RedisIntegerValueGenerator<int>();

            Assert.Equal(1, generator.Next(null));
            Assert.Equal(2, generator.Next(null));
        }

        [Fact]
        public void Can_create_values_for_all_integer_types()
        {
            Assert.Equal(1, new RedisIntegerValueGenerator<int>().Next(null));
            Assert.Equal(1L, new RedisIntegerValueGenerator<long>().Next(null));
            Assert.Equal((short)1, new RedisIntegerValueGenerator<short>().Next(null));
            Assert.Equal(unchecked((byte)1), new RedisIntegerValueGenerator<byte>().Next(null));
            Assert.Equal(unchecked((uint)1), new RedisIntegerValueGenerator<uint>().Next(null));
            Assert.Equal(unchecked((ulong)1), new RedisIntegerValueGenerator<ulong>().Next(null));
            Assert.Equal(unchecked((ushort)1), new RedisIntegerValueGenerator<ushort>().Next(null));
            Assert.Equal((sbyte)1, new RedisIntegerValueGenerator<sbyte>().Next(null));
        }

        [Fact]
        public void Throws_when_type_conversion_would_overflow()
        {
            var generator = new RedisIntegerValueGenerator<byte>();

            for (var i = 1; i < 256; i++)
            {
                generator.Next(null);
            }

            Assert.Throws<OverflowException>(() => generator.Next(null));
        }

        [Fact]
        public void Does_not_generate_temp_values()
        {
            Assert.False(new RedisIntegerValueGenerator<int>().GeneratesTemporaryValues);
        }
    }
}
