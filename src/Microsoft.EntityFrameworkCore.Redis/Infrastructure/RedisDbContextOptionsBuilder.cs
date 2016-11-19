// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class RedisDbContextOptionsBuilder
    {
        public RedisDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            OptionsBuilder = optionsBuilder;
        }

        protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

        public virtual RedisDbContextOptionsBuilder IgnoreTransactions()
            => SetOption(e => e.IgnoreTransactions = true);

        protected virtual RedisDbContextOptionsBuilder SetOption([NotNull] Action<RedisOptionsExtension> setAction)
        {
            Check.NotNull(setAction, nameof(setAction));

            var extension = new RedisOptionsExtension(OptionsBuilder.Options.GetExtension<RedisOptionsExtension>());

            setAction(extension);

            ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);

            return this;
        }
    }
}
