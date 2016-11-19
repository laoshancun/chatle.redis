// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

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
			HostName = copyFrom.HostName;
			Port = copyFrom.Port;
			Database = copyFrom.Database;
			SyncTimeout = copyFrom.SyncTimeout;
			ConnectTimeout = copyFrom.ConnectTimeout;
		}

        public virtual bool IgnoreTransactions
        {
            get { return _ignoreTransactions; }
            set { _ignoreTransactions = value; }
        }

		public virtual string HostName { get; [param: NotNull] set; }
        public virtual int Port { get; set; }
        public virtual int Database { get; set; }
        public virtual int SyncTimeout { get; set; }
        public virtual int ConnectTimeout { get; set; }

        public virtual void ApplyServices(IServiceCollection builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.AddEntityFrameworkRedisDatabase();
        }
    }
}
