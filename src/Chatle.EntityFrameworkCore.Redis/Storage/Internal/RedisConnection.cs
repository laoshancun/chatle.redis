// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
	public class RedisConnection : IRedisConnection
	{
		private readonly string _connectionString;
		private readonly int _database = -1;

		/// <summary>
		///     For testing. Improper usage may lead to NullReference exceptions
		/// </summary>
		protected RedisConnection()
		{
		}

		public RedisConnection([NotNull] IDbContextOptions options)
		{
			Check.NotNull(options, "options");
			var optionsExtension = options.FindExtension<RedisOptionsExtension>();

            HostnPort = $"{optionsExtension.HostName}:{optionsExtension.Port}";
            _connectionString = $"{HostnPort},connectTimeout={optionsExtension.ConnectTimeout},syncTimeout={optionsExtension.SyncTimeout}";
			_database = optionsExtension.Database;
		}

        public virtual string HostnPort { get; private set; }
        public virtual string ConnectionString
		{
			get { return _connectionString; }
		}

		public virtual int Database
		{
			get { return _database; }
		}
	}
}