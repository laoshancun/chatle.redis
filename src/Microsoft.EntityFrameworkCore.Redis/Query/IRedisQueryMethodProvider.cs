using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query
{
    public interface IRedisQueryMethodProvider
    {
		MethodInfo MaterializationQueryMethod { get; }
		MethodInfo ProjectionQueryMethod { get; }
	}
}
