// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal
{
    public class RedisOptionsExtension : IDbContextOptionsExtension
    {
        private bool _ignoreTransactions;

        public RedisOptionsExtension()
        {
        }

        public RedisOptionsExtension([NotNull] RedisOptionsExtension copyFrom)
        {
            _ignoreTransactions = copyFrom._ignoreTransactions;
            ConnectionOptions = copyFrom.ConnectionOptions;
		}

        public virtual bool IgnoreTransactions
        {
            get { return _ignoreTransactions; }
            set { _ignoreTransactions = value; }
        }

        /// <summary>
        /// connection string
        /// </summary>
        public virtual ConfigurationOptions ConnectionOptions { get; set; }

        public virtual void ApplyServices(IServiceCollection builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.AddEntityFrameworkRedisDatabase();
        }
    }
}
