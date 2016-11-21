// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using System.Diagnostics;
using System.Collections;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
	public class RedisQueryModelVisitor : EntityQueryModelVisitor
	{
		private readonly Dictionary<IQuerySource, RedisQuery> _queriesBySource
			= new Dictionary<IQuerySource, RedisQuery>();

		private readonly IRedisMaterializerFactory _materializerFactory;
        private readonly IEntityQueryableExpressionVisitorFactory _entityQueryableExpressionVisitorFactory;
        
        public RedisQueryModelVisitor(
			[NotNull] IQueryOptimizer queryOptimizer,
			[NotNull] INavigationRewritingExpressionVisitorFactory navigationRewritingExpressionVisitorFactory,
			[NotNull] ISubQueryMemberPushDownExpressionVisitor subQueryMemberPushDownExpressionVisitor,
			[NotNull] IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory,
			[NotNull] IEntityResultFindingExpressionVisitorFactory entityResultFindingExpressionVisitorFactory,
			[NotNull] ITaskBlockingExpressionVisitor taskBlockingExpressionVisitor,
			[NotNull] IMemberAccessBindingExpressionVisitorFactory memberAccessBindingExpressionVisitorFactory,
			[NotNull] IOrderingExpressionVisitorFactory orderingExpressionVisitorFactory,
			[NotNull] IProjectionExpressionVisitorFactory projectionExpressionVisitorFactory,
			[NotNull] IEntityQueryableExpressionVisitorFactory entityQueryableExpressionVisitorFactory,
			[NotNull] IQueryAnnotationExtractor queryAnnotationExtractor,
			[NotNull] IResultOperatorHandler resultOperatorHandler,
			[NotNull] IEntityMaterializerSource entityMaterializerSource,
			[NotNull] IExpressionPrinter expressionPrinter,
			[NotNull] IRedisMaterializerFactory materializerFactory,
			[NotNull] RedisQueryCompilationContext queryCompilationContext)
			: base(
				Check.NotNull(queryOptimizer, nameof(queryOptimizer)),
				Check.NotNull(navigationRewritingExpressionVisitorFactory, nameof(navigationRewritingExpressionVisitorFactory)),
				Check.NotNull(subQueryMemberPushDownExpressionVisitor, nameof(subQueryMemberPushDownExpressionVisitor)),
				Check.NotNull(querySourceTracingExpressionVisitorFactory, nameof(querySourceTracingExpressionVisitorFactory)),
				Check.NotNull(entityResultFindingExpressionVisitorFactory, nameof(entityResultFindingExpressionVisitorFactory)),
				Check.NotNull(taskBlockingExpressionVisitor, nameof(taskBlockingExpressionVisitor)),
				Check.NotNull(memberAccessBindingExpressionVisitorFactory, nameof(memberAccessBindingExpressionVisitorFactory)),
				Check.NotNull(orderingExpressionVisitorFactory, nameof(orderingExpressionVisitorFactory)),
				Check.NotNull(projectionExpressionVisitorFactory, nameof(projectionExpressionVisitorFactory)),
				Check.NotNull(entityQueryableExpressionVisitorFactory, nameof(entityQueryableExpressionVisitorFactory)),
				Check.NotNull(queryAnnotationExtractor, nameof(queryAnnotationExtractor)),
				Check.NotNull(resultOperatorHandler, nameof(resultOperatorHandler)),
				Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource)),
				Check.NotNull(expressionPrinter, nameof(expressionPrinter)),
				Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)))
		{
			Check.NotNull(materializerFactory, nameof(materializerFactory));

			_materializerFactory = materializerFactory;
            _entityQueryableExpressionVisitorFactory = entityQueryableExpressionVisitorFactory;

        }

		public override Expression BindMethodCallToValueBuffer([NotNull] MethodCallExpression methodCallExpression, [NotNull] Expression expression)
		{
			Check.NotNull(methodCallExpression, "methodCallExpression");
			Check.NotNull(expression, "expression");

			return BindMethodCallExpression(
				methodCallExpression,
				(property, querySource, redisQuery) =>
				{
					var projectionIndex = redisQuery.GetProjectionIndex(property);
					Debug.Assert(projectionIndex > -1);
					return BindReadValueMethod(methodCallExpression.Type, expression, projectionIndex);
				});
		}

		private TResult BindMethodCallExpression<TResult>(
			MethodCallExpression methodCallExpression,
			Func<IProperty, IQuerySource, RedisQuery, TResult> methodCallBinder)
		{
			return base.BindMethodCallExpression(methodCallExpression, null,
				(property, qs) =>
				{
					var redisQuery = FindOrCreateQuery(qs);
					redisQuery.AddProperty(property);
					return methodCallBinder(property, qs, redisQuery);
				});
		}

		public virtual RedisQuery FindOrCreateQuery([NotNull] IQuerySource querySource)
		{
			Check.NotNull(querySource, "querySource");

			var entityType = QueryCompilationContext.Model.FindEntityType(querySource.ItemType);
			RedisQuery redisQuery;
			if (!_queriesBySource.TryGetValue(querySource, out redisQuery))
			{
				redisQuery = new RedisQuery(entityType);
				_queriesBySource[querySource] = redisQuery;
			}

			return redisQuery;
		}

		public override Expression BindMemberToValueBuffer(
			MemberExpression memberExpression, Expression expression)
		{
			Check.NotNull(memberExpression, "memberExpression");
			Check.NotNull(expression, "expression");

			return BindMemberExpression(
				memberExpression,
				null,
				(property, querySource, redisQuery) =>
				{
					var projectionIndex = redisQuery.GetProjectionIndex(property);
					Debug.Assert(projectionIndex > -1);
					return BindReadValueMethod(memberExpression.Type, expression, projectionIndex);
				});
		}

		private TResult BindMemberExpression<TResult>(
			MemberExpression memberExpression,
			IQuerySource querySource,
			Func<IProperty, IQuerySource, RedisQuery, TResult> memberBinder)
		{
			return base.BindMemberExpression(memberExpression, querySource,
				(property, qs) =>
				{
					var redisQuery = FindOrCreateQuery(qs);
					redisQuery.AddProperty(property);
					return memberBinder(property, qs, redisQuery);
				});
		}

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo OfTypeMethodInfo
            = typeof(Enumerable).GetTypeInfo()
                .GetDeclaredMethod(nameof(Enumerable.OfType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void IncludeNavigations(
            IncludeSpecification includeSpecification,
            Type resultType,
            Expression accessorExpression,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(includeSpecification, nameof(includeSpecification));
            Check.NotNull(resultType, nameof(resultType));

            var includeExpressionVisitor
                = new RedisIncludeExpressionVisitor(
                    _materializerFactory,
                    LinqOperatorProvider,
                    includeSpecification,
                    accessorExpression,
                    querySourceRequiresTracking);

            Expression = includeExpressionVisitor.Visit(Expression);
        }

        private sealed class RedisIncludeExpressionVisitor : ExpressionVisitorBase
        {
            private readonly IncludeSpecification _includeSpecification;
            private readonly IRedisMaterializerFactory _materializerFactory;
            private readonly ILinqOperatorProvider _linqOperatorProvider;
            private readonly Expression _accessorExpression;
            private readonly bool _querySourceRequiresTracking;

            public RedisIncludeExpressionVisitor(
                [NotNull] IRedisMaterializerFactory materializerFactory,
                [NotNull] ILinqOperatorProvider linqOperatorProvider,
                [NotNull] IncludeSpecification includeSpecification,
                [NotNull] Expression accessorExpression,
                bool querySourceRequiresTracking)
            {
                _materializerFactory = materializerFactory;
                _linqOperatorProvider = linqOperatorProvider;
                _includeSpecification = includeSpecification;
                _accessorExpression = accessorExpression;
                _querySourceRequiresTracking = querySourceRequiresTracking;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.MethodIsClosedFormOf(_linqOperatorProvider.Select))
                {
                    var selectorIncludeInjectingVisitor
                        = new SelectorIncludeInjectingExpressionVisitor(
                            _includeSpecification,
                            _materializerFactory,
                            _accessorExpression,
                            _querySourceRequiresTracking);

                    var newSelector = selectorIncludeInjectingVisitor.Visit(node.Arguments[1]);

                    return node.Update(node.Object, new[] { node.Arguments[0], newSelector });
                }

                if (_accessorExpression.Type == node.Type.TryGetSequenceType()
                    || node.Method.MethodIsClosedFormOf(OfTypeMethodInfo))
                {
                    return ApplyTopLevelInclude(node);
                }

                return base.VisitMethodCall(node);
            }

            private Expression ApplyTopLevelInclude(Expression expression)
                => Expression.Call(
                    _includeMethodInfo.MakeGenericMethod(
                        expression.Type.GetTypeInfo().GenericTypeArguments.First()),
                    QueryContextParameter,
                    expression,
                    Expression.Constant(_includeSpecification),
                    Expression.Constant(
                        _includeSpecification.NavigationPath
                            .Select(n =>
                            {
                                var targetType = n.GetTargetType();
                                var materializer = _materializerFactory.CreateMaterializer(targetType);

                                return new RelatedEntitiesLoader(targetType, materializer.Compile());
                            })
                            .ToArray()),
                    Expression.Constant(_querySourceRequiresTracking));

            private class SelectorIncludeInjectingExpressionVisitor : ExpressionVisitorBase
            {
                private readonly IncludeSpecification _includeSpecification;
                private readonly IRedisMaterializerFactory _materializerFactory;
                private readonly Expression _accessorExpression;
                private readonly bool _querySourceRequiresTracking;

                public SelectorIncludeInjectingExpressionVisitor(
                    IncludeSpecification includeSpecification,
                    IRedisMaterializerFactory materializerFactory,
                    Expression accessorExpression,
                    bool querySourceRequiresTracking)
                {
                    _includeSpecification = includeSpecification;
                    _materializerFactory = materializerFactory;
                    _accessorExpression = accessorExpression;
                    _querySourceRequiresTracking = querySourceRequiresTracking;
                }

                public override Expression Visit(Expression node)
                {
                    if (node != null
                        && node == _accessorExpression)
                    {
                        var includeMethod = _includeSpecification.IsEnumerableTarget
                            ? _includeCollectionMethodInfo.MakeGenericMethod(node.Type)
                            : _includeEntityMethodInfo.MakeGenericMethod(node.Type);

                        var result = Expression.Call(
                            includeMethod,
                            QueryContextParameter,
                            node,
                            Expression.Constant(_includeSpecification),
                            Expression.Constant(
                                _includeSpecification.NavigationPath
                                    .Select(n =>
                                    {
                                        var targetType = n.GetTargetType();
                                        var materializer = _materializerFactory.CreateMaterializer(targetType);

                                        return new RelatedEntitiesLoader(targetType, materializer.Compile());
                                    })
                                    .ToArray()),
                            Expression.Constant(_querySourceRequiresTracking));

                        return result;
                    }

                    return base.Visit(node);
                }

                protected override Expression VisitLambda<T>(Expression<T> node)
                {
                    var newBody = Visit(node.Body);

                    return node.Update(newBody, node.Parameters);
                }
            }
        }

        private sealed class RelatedEntitiesLoader : IRelatedEntitiesLoader, IAsyncRelatedEntitiesLoader
        {
            private readonly IEntityType _targetType;
            private readonly Func<IEntityType, ValueBuffer, object> _materializer;

            public RelatedEntitiesLoader(IEntityType targetType, Func<IEntityType, ValueBuffer, object> materializer)
            {
                _targetType = targetType;
                _materializer = materializer;
            }

            public IEnumerable<EntityLoadInfo> Load(QueryContext queryContext, IIncludeKeyComparer keyComparer)
            {
                var result = ((RedisQueryContext)queryContext).Store
                    .GetResultsEnumerable(new RedisQuery(_targetType))
                    .Select(vs => new EntityLoadInfo(
                                new ValueBuffer(vs), vb => _materializer(_targetType, vb)))
                    .Where(eli => keyComparer.ShouldInclude(eli.ValueBuffer));

                return result;
            }

            IAsyncEnumerable<EntityLoadInfo> IAsyncRelatedEntitiesLoader.Load(QueryContext queryContext, IIncludeKeyComparer keyComparer)
            {
                var result = ((RedisQueryContext)queryContext).Store
                    .GetResultsAsyncEnumerable(new RedisQuery(_targetType))
                    .Select(vs => new EntityLoadInfo(
                                new ValueBuffer(vs), vb => _materializer(_targetType, vb)))
                    .Where(eli => keyComparer.ShouldInclude(eli.ValueBuffer));

                return result;
            }

            public void Dispose()
            {
                // no-op
            }
        }

        private static readonly MethodInfo _includeEntityMethodInfo
            = typeof(RedisQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(IncludeEntity));

        private static readonly MethodInfo _includeCollectionMethodInfo
            = typeof(RedisQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(IncludeCollection));

        private static TEntity IncludeEntity<TEntity>(
            QueryContext queryContext,
            TEntity source,
            IncludeSpecification includeSpecification,
            IReadOnlyList<IRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool querySourceRequiresTracking)
        {
            queryContext.QueryBuffer
                .Include(
                    queryContext,
                    source,
                    includeSpecification.NavigationPath,
                    relatedEntitiesLoaders,
                    querySourceRequiresTracking);

            return source;
        }

        [UsedImplicitly]
        private static TEntity IncludeCollection<TEntity>(
            QueryContext queryContext,
            TEntity source,
            IncludeSpecification includeSpecification,
            IReadOnlyList<IRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool querySourceRequiresTracking)
        {
            foreach (var entity in (IEnumerable)source)
            {
                queryContext.QueryBuffer
                    .Include(
                        queryContext,
                        entity,
                        includeSpecification.NavigationPath,
                        relatedEntitiesLoaders,
                        querySourceRequiresTracking);
            }

            return source;
        }

        private static readonly MethodInfo _includeMethodInfo
            = typeof(RedisQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(Include));

        [UsedImplicitly]
        private static IEnumerable<TResult> Include<TResult>(
            QueryContext queryContext,
            IEnumerable<TResult> source,
            IncludeSpecification includeSpecification,
            IReadOnlyList<IRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool querySourceRequiresTracking)
        {
            foreach (var result in source)
            {
                if (includeSpecification.IsEnumerableTarget)
                {
                    foreach (var entity in (IEnumerable)result)
                    {
                        queryContext.QueryBuffer
                            .Include(
                                queryContext,
                                entity,
                                includeSpecification.NavigationPath,
                                relatedEntitiesLoaders,
                                querySourceRequiresTracking);
                    }
                }
                else
                {
                    queryContext.QueryBuffer
                        .Include(
                            queryContext,
                            result,
                            includeSpecification.NavigationPath,
                            relatedEntitiesLoaders,
                            querySourceRequiresTracking);
                }

                yield return result;
            }
        }
    }
}
