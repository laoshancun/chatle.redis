// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public class RedisEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        private readonly IModel _model;
        private readonly IRedisMaterializerFactory _materializerFactory;
        private readonly IQuerySource _querySource;

        public RedisEntityQueryableExpressionVisitor(
            [NotNull] IModel model,
            [NotNull] IRedisMaterializerFactory materializerFactory,
            [NotNull] RedisQueryModelVisitor entityQueryModelVisitor,
            [CanBeNull] IQuerySource querySource)
            : base(Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)))
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(materializerFactory, nameof(materializerFactory));

            _model = model;
            _materializerFactory = materializerFactory;
            _querySource = querySource;
        }

        private new RedisQueryModelVisitor QueryModelVisitor
            => (RedisQueryModelVisitor)base.QueryModelVisitor;

        protected override Expression VisitEntityQueryable(Type elementType)
        {
            var redisQuery = QueryModelVisitor.FindOrCreateQuery(_querySource);
            var entityType = _model.FindEntityType(elementType);
            var methodProvider =
                    ((RedisQueryCompilationContext)QueryModelVisitor.QueryCompilationContext).QueryMethodProvider;

            if (QueryModelVisitor.QueryCompilationContext
                .QuerySourceRequiresMaterialization(_querySource))
            {
                var materializer = _materializerFactory.CreateMaterializer(entityType);

                return Expression.Call(
                    methodProvider.MaterializationQueryMethod.MakeGenericMethod(elementType),
                    EntityQueryModelVisitor.QueryContextParameter,
                    Expression.Constant(redisQuery),
                    Expression.Constant(entityType.FindPrimaryKey()),
                    materializer,
                    Expression.Constant(QueryModelVisitor.QueryCompilationContext.IsTrackingQuery));
            }

            return Expression.Call(
                methodProvider.ProjectionQueryMethod,
                EntityQueryModelVisitor.QueryContextParameter,
                Expression.Constant(redisQuery));

        }
    }
}
