// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class RedisIntegerValueGeneratorFactory : ValueGeneratorFactory
    {
        public override ValueGenerator Create(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var type = property.ClrType.UnwrapNullableType().UnwrapEnumType();

            if (type == typeof(long))
            {
                return new RedisIntegerValueGenerator<long>();
            }

            if (type == typeof(int))
            {
                return new RedisIntegerValueGenerator<int>();
            }

            if (type == typeof(short))
            {
                return new RedisIntegerValueGenerator<short>();
            }

            if (type == typeof(byte))
            {
                return new RedisIntegerValueGenerator<byte>();
            }

            if (type == typeof(ulong))
            {
                return new RedisIntegerValueGenerator<ulong>();
            }

            if (type == typeof(uint))
            {
                return new RedisIntegerValueGenerator<uint>();
            }

            if (type == typeof(ushort))
            {
                return new RedisIntegerValueGenerator<ushort>();
            }

            if (type == typeof(sbyte))
            {
                return new RedisIntegerValueGenerator<sbyte>();
            }

            throw new ArgumentException(CoreStrings.InvalidValueGeneratorFactoryProperty(
                nameof(RedisIntegerValueGeneratorFactory), property.Name, property.DeclaringEntityType.DisplayName()));
        }
    }
}
