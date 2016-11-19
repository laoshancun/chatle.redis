// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query
{
	public class RedisQueryMethodProvider : IRedisQueryMethodProvider
	{
		public virtual MethodInfo MaterializationQueryMethod
		{
			get { return _executeMaterializationQueryExpressionMethodInfo; }
		}

		public virtual MethodInfo ProjectionQueryMethod
		{
			get { return _executeProjectionQueryExpressionMethodInfo; }
		}

		private static readonly MethodInfo
			_executeMaterializationQueryExpressionMethodInfo =
				typeof(RedisQueryMethodProvider).GetTypeInfo()
					.GetDeclaredMethod("ExecuteMaterializationQueryExpression");

		[UsedImplicitly]
		private static IEnumerable<TEntity> ExecuteMaterializationQueryExpression<TEntity>(
			QueryContext queryContext,
			RedisQuery redisQuery,
			IKey key,
			Func<IEntityType, ValueBuffer, object> materializer,
			bool queryStateManager)
			where TEntity : class, new()
		{
			var redisQueryContext = (RedisQueryContext)queryContext;

			return redisQueryContext
				.GetResultsEnumerable(redisQuery)
				.Select(objectArray =>
				{

					var valueReader = new ValueBuffer(objectArray);					

					return (TEntity)redisQueryContext.QueryBuffer
						.GetEntity(key, new EntityLoadInfo(valueReader, vr => materializer(redisQuery.EntityType, vr)), queryStateManager, true);
				});
		}

		private static readonly MethodInfo
			_executeProjectionQueryExpressionMethodInfo =
				typeof(RedisQueryMethodProvider).GetTypeInfo()
					.GetDeclaredMethod("ExecuteProjectionQueryExpression");

		[UsedImplicitly]
		private static IEnumerable<ValueBuffer> ExecuteProjectionQueryExpression(
			QueryContext queryContext, RedisQuery redisQuery)
		{
			var redisQueryContext = (RedisQueryContext)queryContext;

			return redisQueryContext
				.GetResultsEnumerable(redisQuery)
				.Select(objectArray => new ValueBuffer(objectArray));
		}
	}
}