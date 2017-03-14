using StackExchange.Redis;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
	public interface IRedisConnection
	{
        ConfigurationOptions ConnectionOptions { get; }
		int Database { get; }
	}
}