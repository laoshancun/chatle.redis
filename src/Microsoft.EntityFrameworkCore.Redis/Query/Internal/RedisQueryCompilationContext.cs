using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class RedisQueryCompilationContext: QueryCompilationContext
	{
		private readonly IRedisQueryMethodProvider _queryMethodProvider;
		public RedisQueryCompilationContext(
			[NotNull] IModel model,
			[NotNull] ISensitiveDataLogger logger,
			[NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory,
			[NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory,
			[NotNull] ILinqOperatorProvider linqOperatorProvider,
			[NotNull] IRedisQueryMethodProvider queryMethodProvider,
			[NotNull] Type contextType,
			bool trackQueryResults)
			: base(
				Check.NotNull(model, nameof(model)),
				Check.NotNull(logger, nameof(logger)),
				Check.NotNull(entityQueryModelVisitorFactory, nameof(entityQueryModelVisitorFactory)),
				Check.NotNull(requiresMaterializationExpressionVisitorFactory, nameof(requiresMaterializationExpressionVisitorFactory)),
				Check.NotNull(linqOperatorProvider, nameof(linqOperatorProvider)),
				Check.NotNull(contextType, nameof(contextType)),
				trackQueryResults)
		{
			Check.NotNull(queryMethodProvider, nameof(queryMethodProvider));
			_queryMethodProvider = queryMethodProvider;
		}

		public virtual IRedisQueryMethodProvider QueryMethodProvider
		{
			get { return _queryMethodProvider; }
		}
	}
}
