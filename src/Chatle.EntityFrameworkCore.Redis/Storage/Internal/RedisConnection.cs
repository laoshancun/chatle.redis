// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using StackExchange.Redis;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
	public class RedisConnection : IRedisConnection
	{
		private readonly ConfigurationOptions _options;
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


            _database=optionsExtension.ConnectionOptions.DefaultDatabase.Value;

            _options = optionsExtension.ConnectionOptions;

        }

        public virtual ConfigurationOptions ConnectionOptions
        {
			get { return _options; }
		}

		public virtual int Database
		{
			get { return _database; }
		}
	}
}