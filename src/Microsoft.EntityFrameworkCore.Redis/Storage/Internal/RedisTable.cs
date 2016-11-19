// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Update;
using StackExchange.Redis;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class RedisTable<TKey> : IRedisTable
    {
        private readonly IPrincipalKeyValueFactory<TKey> _keyValueFactory;
        private readonly Dictionary<TKey, object[]> _rows;

        public RedisTable([NotNull] ConnectionMultiplexer connectionMutiplexer, [NotNull] IPrincipalKeyValueFactory<TKey> keyValueFactory)
        {
            _keyValueFactory = keyValueFactory;
            _rows = new Dictionary<TKey, object[]>(keyValueFactory.EqualityComparer);
        }

        public virtual IReadOnlyList<object[]> SnapshotRows()
            => _rows.Values.ToList();

        public virtual void Create(IUpdateEntry entry)
            => _rows.Add(CreateKey(entry), CreateValueBuffer(entry));

        public virtual void Delete(IUpdateEntry entry)
            => _rows.Remove(CreateKey(entry));

        public virtual void Update(IUpdateEntry entry)
            => _rows[CreateKey(entry)] = CreateValueBuffer(entry);

        private TKey CreateKey(IUpdateEntry entry)
            => _keyValueFactory.CreateFromCurrentValues((InternalEntityEntry)entry);

        private static object[] CreateValueBuffer(IUpdateEntry entry)
            => entry.EntityType.GetProperties().Select(entry.GetCurrentValue).ToArray();
    }
}
