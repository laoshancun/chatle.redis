namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
	public interface IRedisConnection
	{
		string ConnectionString { get; }
		int Database { get; }
	}
}