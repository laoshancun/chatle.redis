// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class RedisDatabase : Database, IRedisDatabase
    {
        private readonly IRedisStore _database;

        public RedisDatabase(
            [NotNull] IQueryCompilationContextFactory queryCompilationContextFactory,
            [NotNull] IRedisStore persistentStore,
            [NotNull] IDbContextOptions options)
            : base(queryCompilationContextFactory)
        {
            Check.NotNull(queryCompilationContextFactory, nameof(queryCompilationContextFactory));
            Check.NotNull(persistentStore, nameof(persistentStore));
            Check.NotNull(options, nameof(options));

            _database = persistentStore;
        }

        public virtual IRedisStore Store => _database;

        public override int SaveChanges(IReadOnlyList<IUpdateEntry> entries)
            => _database.ExecuteTransaction(Check.NotNull(entries, nameof(entries)));

        public override Task<int> SaveChangesAsync(
            IReadOnlyList<IUpdateEntry> entries,
            CancellationToken cancellationToken = default(CancellationToken))
            => _database.ExecuteTransactionAsync(Check.NotNull(entries, nameof(entries)));

        public virtual bool EnsureDatabaseCreated(IModel model)
            => _database.EnsureCreated(Check.NotNull(model, nameof(model)));
       
    }
}
