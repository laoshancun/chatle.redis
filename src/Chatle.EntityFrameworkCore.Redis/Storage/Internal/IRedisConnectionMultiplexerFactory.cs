using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public interface IRedisConnectionMultiplexerFactory
    {
        ConnectionMultiplexer GetOrCreate([NotNull] IRedisConnection connection);
    }
}
