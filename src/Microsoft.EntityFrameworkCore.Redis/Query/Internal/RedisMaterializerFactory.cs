using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
	public class RedisMaterializerFactory : IRedisMaterializerFactory
	{
		private readonly IEntityMaterializerSource _entityMaterializerSource;

		public RedisMaterializerFactory([NotNull] IEntityMaterializerSource entityMaterializerSource)
		{
			Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));

			_entityMaterializerSource = entityMaterializerSource;
		}

        public virtual Expression<Func<IEntityType, ValueBuffer, object>> CreateMaterializer(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var entityTypeParameter
                = Expression.Parameter(typeof(IEntityType), "entityType");

            var valueBufferParameter
                = Expression.Parameter(typeof(ValueBuffer), "valueBuffer");

            var concreteEntityTypes
                = entityType.GetConcreteTypesInHierarchy().ToList();

            if (concreteEntityTypes.Count == 1)
            {
                return Expression.Lambda<Func<IEntityType, ValueBuffer, object>>(
                    _entityMaterializerSource
                        .CreateMaterializeExpression(
                            concreteEntityTypes[0], valueBufferParameter),
                    entityTypeParameter,
                    valueBufferParameter);
            }

            var returnLabelTarget = Expression.Label(typeof(object));

            var blockExpressions
                = new Expression[]
                {
                    Expression.IfThen(
                        Expression.Equal(
                            entityTypeParameter,
                            Expression.Constant(concreteEntityTypes[0])),
                        Expression.Return(
                            returnLabelTarget,
                            _entityMaterializerSource
                                .CreateMaterializeExpression(
                                    concreteEntityTypes[0], valueBufferParameter))),
                    Expression.Label(
                        returnLabelTarget,
                        Expression.Default(returnLabelTarget.Type))
                };

            foreach (var concreteEntityType in concreteEntityTypes.Skip(1))
            {
                blockExpressions[0]
                    = Expression.IfThenElse(
                        Expression.Equal(
                            entityTypeParameter,
                            Expression.Constant(concreteEntityType)),
                        Expression.Return(
                            returnLabelTarget,
                            _entityMaterializerSource
                                .CreateMaterializeExpression(concreteEntityType, valueBufferParameter)),
                        blockExpressions[0]);
            }

            return Expression.Lambda<Func<IEntityType, ValueBuffer, object>>(
                Expression.Block(blockExpressions),
                entityTypeParameter,
                valueBufferParameter);
        }
    }
}
