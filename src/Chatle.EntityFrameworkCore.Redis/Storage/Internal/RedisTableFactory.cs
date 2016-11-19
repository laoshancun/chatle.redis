// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StackExchange.Redis;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
	public class RedisTableFactory : IdentityMapFactoryFactoryBase, IRedisTableFactory
	{
		private readonly ConcurrentDictionary<IKey, Func<ConnectionMultiplexer, IRedisTable>> _factories
			= new ConcurrentDictionary<IKey, Func<ConnectionMultiplexer, IRedisTable>>();

		public virtual IRedisTable Create(ConnectionMultiplexer connectionMutiplexer, IEntityType entityType)
			=> _factories.GetOrAdd(entityType.FindPrimaryKey(), key => Create(connectionMutiplexer, key))(connectionMutiplexer);

		private Func<ConnectionMultiplexer, IRedisTable> Create([NotNull] ConnectionMultiplexer connectionMutiplexer, [NotNull] IKey key)
			=> (Func<ConnectionMultiplexer, IRedisTable>)typeof(RedisTableFactory).GetTypeInfo()
				.GetDeclaredMethods(nameof(CreateFactory)).Single()
				.MakeGenericMethod(GetKeyType(key))
				.Invoke(null, new object[] { connectionMutiplexer, key });

		[UsedImplicitly]
		private static Func<IRedisTable> CreateFactory<TKey>(ConnectionMultiplexer connectionMutiplexer, IKey key)
			=> () => new RedisTable<TKey>(connectionMutiplexer, key.GetPrincipalKeyValueFactory<TKey>());
	}
}
