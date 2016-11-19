// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class RedisValueGeneratorSelector : ValueGeneratorSelector
    {
        private readonly RedisIntegerValueGeneratorFactory _inMemoryFactory = new RedisIntegerValueGeneratorFactory();

        public RedisValueGeneratorSelector([NotNull] IValueGeneratorCache cache)
            : base(cache)
        {
        }

        public override ValueGenerator Create(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.ClrType.IsInteger()
                ? _inMemoryFactory.Create(property)
                : base.Create(property, entityType);
        }
    }
}
