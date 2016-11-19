// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class RedisDatabaseProviderServices : DatabaseProviderServices
    {
        public RedisDatabaseProviderServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override string InvariantName => GetType().GetTypeInfo().Assembly.GetName().Name;
        public override IDatabase Database => GetService<IRedisDatabase>();
        public override IDbContextTransactionManager TransactionManager => GetService<RedisTransactionManager>();
        public override IQueryContextFactory QueryContextFactory => GetService<RedisQueryContextFactory>();
        public override IDatabaseCreator Creator => GetService<RedisDatabaseCreator>();
        public override IValueGeneratorSelector ValueGeneratorSelector => GetService<RedisValueGeneratorSelector>();
        public override IModelSource ModelSource => GetService<RedisModelSource>();
        public override IValueGeneratorCache ValueGeneratorCache => GetService<RedisValueGeneratorCache>();
        public override IEntityQueryableExpressionVisitorFactory EntityQueryableExpressionVisitorFactory => GetService<RedisEntityQueryableExpressionVisitorFactory>();
        public override IEntityQueryModelVisitorFactory EntityQueryModelVisitorFactory => GetService<RedisQueryModelVisitorFactory>();
        public override IQueryCompilationContextFactory QueryCompilationContextFactory => GetService<RedisQueryCompilationContextFactory>();
    }
}
