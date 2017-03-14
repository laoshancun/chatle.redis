using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
	public class RedisConnectionMultipexerFactory : IRedisConnectionMultiplexerFactory
	{
		private readonly ConcurrentDictionary<string, ConnectionMultiplexer> _connectionMultiplexers
			= new ConcurrentDictionary<string, ConnectionMultiplexer>();

		public virtual ConnectionMultiplexer GetOrCreate([NotNull] IRedisConnection connection)
		{
            var configurationOptions = connection.ConnectionOptions;
            configurationOptions.AllowAdmin = true; // require Admin access for Server commands

			var connectionMultiplexerKey = configurationOptions.ToString();

			ConnectionMultiplexer connectionMultiplexer;

			if (!_connectionMultiplexers.TryGetValue(connectionMultiplexerKey, out connectionMultiplexer))
			{
				connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);

				if (!_connectionMultiplexers.TryAdd(connectionMultiplexerKey, connectionMultiplexer))
				{
					connectionMultiplexer = _connectionMultiplexers[connectionMultiplexerKey];
				}
			}

			return connectionMultiplexer;
		}
	}
}
