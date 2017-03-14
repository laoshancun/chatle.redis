// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Logging;
using Chatle.EntityFrameworkCore.Redis.Properties;
using StackExchange.Redis;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using System.Globalization;
using System.Text;
using Remotion.Linq;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class RedisStore : IRedisStore
    {
		private const string KeyNameSeparator = ":";
		private const string EscapedKeyNameSeparator = @"\x3A";
		private const string PropertyValueSeparator = "::";

		private const string EntityFrameworkPrefix = "EF" + KeyNameSeparator;

		private const string IndexPrefix =
			EntityFrameworkPrefix + "Index" + KeyNameSeparator;

		private const string PrimaryKeyIndexPrefix =
			IndexPrefix + "PK" + KeyNameSeparator;

		private const string PrimaryKeyIndexNameFormat = // argument is EntityType
			PrimaryKeyIndexPrefix + "{0}";

		private const string DataPrefix =
			EntityFrameworkPrefix + "Data" + KeyNameSeparator;

		private const string DataHashNameFormat = // 1st arg is EntityType, 2nd is value of the PK
			DataPrefix + "{0}" + KeyNameSeparator + "{1}";

		private const string ValueGeneratorPrefix =
			EntityFrameworkPrefix + "ValueGenerator" + KeyNameSeparator;

		private const string ValueGeneratorKeyNameFormat = // 1st arg is EntityType, 2nd is name of the property
			ValueGeneratorPrefix + "{0}" + KeyNameSeparator + "{1}";

        private readonly ILogger _logger;
		private readonly IRedisConnectionMultiplexerFactory _multiplexerFactory;
		private readonly IRedisTableFactory _tableFactory;

		public RedisStore(
			[NotNull] IRedisTableFactory tableFactory,
			[NotNull] IRedisConnectionMultiplexerFactory multiplexerFactory,
            [NotNull] IRedisConnection connection,
            [NotNull] ILogger<RedisStore> logger)
        {
            Connection = connection;
            _logger = logger;
			_multiplexerFactory = multiplexerFactory;
			_tableFactory = tableFactory;
		}
		public virtual IRedisConnection Connection { get; private set; }

		public virtual StackExchange.Redis.IDatabase GetUnderlyingDatabase()
		{
			return ConnectionMultiplexer
				.GetDatabase(Connection.Database);
		}

		public virtual IServer GetUnderlyingServer()
		{
			return ConnectionMultiplexer
				.GetServer(((RedisConnection)Connection).ConnectionOptions.EndPoints.First());

		}

		private ConnectionMultiplexer ConnectionMultiplexer
		{
			get
			{
				return _multiplexerFactory.GetOrCreate(Connection);
			}
		}

		/// <summary>
		///     Returns true just after the database has been created, false thereafter
		/// </summary>
		/// <returns>
		///     true if the database has just been created, false otherwise
		/// </returns>
		public virtual bool EnsureCreated(IModel model)
        {
			try
			{
				GetUnderlyingServer();
			}
            catch (Exception e)
            {
                _logger.LogError(null, e, e.Message);
				return false;
			}

			return true;
        }

		/// <summary>
		///     Deletes all keys in the database
		/// </summary>
		public virtual bool FlushDatabase()
		{
			try
			{
				GetUnderlyingServer().FlushDatabase(Connection.Database);
			}
			catch(Exception e)
			{
                _logger.LogError(null, e, e.Message);
				return false;
			}

			return true;
		}

		/// <summary>
		///     Deletes all keys in the database
		/// </summary>
		public virtual async Task<bool> FlushDatabaseAsync(
			CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				await GetUnderlyingServer().FlushDatabaseAsync(Connection.Database);
			}
			catch
			{
				return false;
			}

			return true;
		}

		public virtual int ExecuteTransaction(
			[NotNull] IEnumerable<IUpdateEntry> stateEntries)
		{
			Check.NotNull(stateEntries, "stateEntries");
			var transaction = PrepareTransactionForSaveChanges(stateEntries);
			var entitiesProcessed = 0;
			if (transaction.Execute())
			{
				entitiesProcessed = stateEntries.Count();
			}
			return entitiesProcessed;
		}

		public virtual async Task<int> ExecuteTransactionAsync(
			[NotNull] IEnumerable<IUpdateEntry> stateEntries,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			Check.NotNull(stateEntries, "stateEntries");
			cancellationToken.ThrowIfCancellationRequested();
			var transaction = PrepareTransactionForSaveChanges(stateEntries);
			var entitiesProcessed = 0;
			if (await transaction.ExecuteAsync())
			{
				entitiesProcessed = stateEntries.Count();
			}
            _logger.LogInformation(RedisEventId.SavedChanges, entitiesProcessed, s => s.ToString());

			return entitiesProcessed;
		}

		private ITransaction PrepareTransactionForSaveChanges(IEnumerable<IUpdateEntry> stateEntries)
		{
			var transaction = GetUnderlyingDatabase().CreateTransaction();
			foreach (var entry in stateEntries)
			{
				switch (entry.EntityState)
				{
					case EntityState.Added:
						AddInsertEntryCommands(transaction, entry);
						break;
					case EntityState.Deleted:
						AddDeleteEntryCommands(transaction, entry);
						break;
					case EntityState.Modified:
						AddModifyEntryCommands(transaction, entry);
						break;
				}
			}
			return transaction;
		}
		
		/// <summary>
		///     Gets a set of object[] values from database each of which represents the values
		///     of the Properties required by the query for a particular EntityType
		/// </summary>
		/// <param name="redisQuery">An object representing the parameters of the query</param>
		/// <returns>
		///     An Enumerable of object[] values from database each of which represents
		///     the values of the Properties for the EntityType (either all the propoerties
		///     or the selected properties as defined by the query)
		/// </returns>
		public virtual IEnumerable<object[]> GetResultsEnumerable([NotNull] RedisQuery redisQuery)
		{
			Check.NotNull(redisQuery, "redisQuery");
			var redisPrimaryKeyIndexKeyName
				= ConstructRedisPrimaryKeyIndexKeyName(redisQuery.EntityType);
			var allKeysForEntity
				= GetUnderlyingDatabase().SetMembers(redisPrimaryKeyIndexKeyName);
			return allKeysForEntity
				.Select(compositePrimaryKey
					=> GetQueryObjectsFromDatabase(
						compositePrimaryKey, redisQuery, DecodeBytes));
		}
		
		/// <summary>
		///     Gets a set of object[] values from database each of which represents the values
		///     of the Properties required by the query for a particular EntityType
		/// </summary>
		/// <param name="redisQuery">An object representing the parameters of the query</param>
		/// <returns>
		///     An Enumerable of object[] values from database each of which represents
		///     the values of the Properties for the EntityType (either all the propoerties
		///     or the selected properties as defined by the query)
		/// </returns>
		public virtual IAsyncEnumerable<object[]> GetResultsAsyncEnumerable([NotNull] RedisQuery redisQuery)
		{
			Check.NotNull(redisQuery, "redisQuery");
			return new AsyncEnumerable(this, redisQuery);
		}

		private void AddInsertEntryCommands(ITransaction transaction, IUpdateEntry stateEntry)
		{
			var compositePrimaryKeyValues =
				ConstructKeyValue(stateEntry, (se, prop) => stateEntry.GetCurrentValue(prop));
			var redisDataKeyName =
				ConstructRedisDataKeyName(stateEntry.EntityType, compositePrimaryKeyValues);
			// Note: null entries are stored as the absence of the property_name-property_value pair in the hash
			var entries =
				stateEntry.EntityType.GetProperties()
					.Where(p => stateEntry.GetCurrentValue(p) != null)
					.Select(p => new HashEntry(p.Name, EncodeAsBytes(stateEntry.GetCurrentValue(p)))).ToArray();
			transaction.HashSetAsync(redisDataKeyName, entries);
			var redisPrimaryKeyIndexKeyName = ConstructRedisPrimaryKeyIndexKeyName(stateEntry.EntityType);
			transaction.SetAddAsync(redisPrimaryKeyIndexKeyName, compositePrimaryKeyValues);
		}

		private void AddDeleteEntryCommands(ITransaction transaction, IUpdateEntry stateEntry)
		{
			var compositePrimaryKeyValues =
				ConstructKeyValue(stateEntry, (se, prop) => stateEntry.GetOriginalValue(prop));
			var redisDataKeyName = ConstructRedisDataKeyName(stateEntry.EntityType, compositePrimaryKeyValues);
			transaction.KeyDeleteAsync(redisDataKeyName);
			var redisPrimaryKeyIndexKeyName = ConstructRedisPrimaryKeyIndexKeyName(stateEntry.EntityType);
			transaction.SetRemoveAsync(redisPrimaryKeyIndexKeyName, compositePrimaryKeyValues);
		}

		private void AddModifyEntryCommands(ITransaction transaction, IUpdateEntry stateEntry)
		{
			var compositePrimaryKeyValues =
				ConstructKeyValue(stateEntry, (se, prop) => stateEntry.GetOriginalValue(prop));
			var redisPrimaryKeyIndexKeyName = ConstructRedisPrimaryKeyIndexKeyName(stateEntry.EntityType);
			var redisDataKeyName = ConstructRedisDataKeyName(stateEntry.EntityType, compositePrimaryKeyValues);
			transaction.AddCondition(Condition.KeyExists(redisPrimaryKeyIndexKeyName));
			// first delete all the hash entries which have changed to null
			var changingToNullEntries = stateEntry.EntityType.GetProperties()
				.Where(p => stateEntry.IsModified(p) && stateEntry.GetCurrentValue(p) == null)
				.Select(p => (RedisValue)p.Name).ToArray();
			transaction.HashDeleteAsync(redisDataKeyName, changingToNullEntries);
			// now update all the other entries
			var updatedEntries = stateEntry.EntityType.GetProperties()
				.Where(p => stateEntry.IsModified(p) && stateEntry.GetCurrentValue(p) != null)
				.Select(p => new HashEntry(p.Name, EncodeAsBytes(stateEntry.GetCurrentValue(p)))).ToArray();
			transaction.HashSetAsync(redisDataKeyName, updatedEntries);
		}

		private static string ConstructRedisPrimaryKeyIndexKeyName(IEntityType entityType)
		{
			return string.Format(CultureInfo.InvariantCulture,
				PrimaryKeyIndexNameFormat, Escape(entityType.Name));
		}

		private static string ConstructRedisDataKeyName(
			IEntityType entityType, string compositePrimaryKeyValues)
		{
			return string.Format(CultureInfo.InvariantCulture,
				DataHashNameFormat, Escape(entityType.Name), compositePrimaryKeyValues);
		}

		public static string ConstructRedisValueGeneratorKeyName([NotNull] IProperty property)
		{
			Check.NotNull(property, "property");
			return string.Format(CultureInfo.InvariantCulture,
				ValueGeneratorKeyNameFormat, Escape(property.DeclaringEntityType.Name), Escape(property.Name));
		}

		private static string ConstructKeyValue(
			IUpdateEntry stateEntry, Func<IUpdateEntry, IProperty, object> propertyValueSelector)
		{
			return string.Join(
				PropertyValueSeparator,
				stateEntry.EntityType.FindPrimaryKey().Properties.Select(p => EncodeKeyValue(propertyValueSelector(stateEntry, p))));
		}

		// returns the object array representing all the properties
		// required by the RedisQuery. Note: if SelectedProperties is
		// null or empty then return all properties.
		private object[] GetQueryObjectsFromDatabase(
			string primaryKey, RedisQuery redisQuery, Func<byte[], IProperty, object> decoder)
		{
			object[] results = null;
			var dataKeyName = ConstructRedisDataKeyName(redisQuery.EntityType, primaryKey);
			if (redisQuery.SelectedProperties == null
				|| !redisQuery.SelectedProperties.Any())
			{
				results = new object[redisQuery.EntityType.GetProperties().Count()];
				// HGETALL (all properties)
				var redisHashEntries = GetUnderlyingDatabase().HashGetAll(dataKeyName)
					.ToDictionary(he => he.Name, he => he.Value);
				foreach (var property in redisQuery.EntityType.GetProperties())
				{
					// Note: since null's are stored in the database as the absence of the column name in the hash
					// need to insert null's into the objectArray at the appropriate places.
					RedisValue propertyRedisValue;
					results[property.GetIndex()] =
						redisHashEntries.TryGetValue(property.Name, out propertyRedisValue)
							? decoder(propertyRedisValue, property)
							: null;
				}
			}
			else
			{
				var selectedPropertiesArray = redisQuery.SelectedProperties.ToArray();
				results = new object[selectedPropertiesArray.Length];
				// HMGET (selected properties)
				var fields = selectedPropertiesArray.Select(p => (RedisValue)p.Name).ToArray();
				var redisHashEntries = GetUnderlyingDatabase().HashGet(dataKeyName, fields);
				for (var i = 0; i < selectedPropertiesArray.Length; i++)
				{
					results[i] =
						redisHashEntries[i].IsNull
							? null
							: decoder(redisHashEntries[i], selectedPropertiesArray[i]);
				}
			}
			return results;
		}

		// returns the object array representing all the properties
		// from an EntityType with a particular primary key
		private async Task<object[]> GetQueryObjectsFromDatabaseAsync(
			string primaryKey, RedisQuery redisQuery, Func<byte[], IProperty, object> decoder)
		{
			object[] results = null;
			var dataKeyName = ConstructRedisDataKeyName(redisQuery.EntityType, primaryKey);
			if (redisQuery.SelectedProperties == null
				|| !redisQuery.SelectedProperties.Any())
			{
				results = new object[redisQuery.EntityType.GetProperties().Count()];
				// Async HGETALL
				var redisHashEntries = await GetUnderlyingDatabase().HashGetAllAsync(dataKeyName);
				foreach (var property in redisQuery.EntityType.GetProperties())
				{
					var redisHashEntriesDictionary = redisHashEntries.ToDictionary(he => he.Name, he => he.Value);
					// Note: since null's are stored in the database as the absence of the column name in the hash
					// need to insert null's into the objectArray at the appropriate places.
					RedisValue propertyRedisValue;
					if (redisHashEntriesDictionary.TryGetValue(property.Name, out propertyRedisValue))
					{
						results[property.GetIndex()] = decoder(propertyRedisValue, property);
					}
					else
					{
						results[property.GetIndex()] = null;
					}
				}
			}
			else
			{
				var selectedPropertiesArray = redisQuery.SelectedProperties.ToArray();
				results = new object[selectedPropertiesArray.Length];
				// Async HMGET
				var fields = selectedPropertiesArray.Select(p => (RedisValue)p.Name).ToArray();
				var redisHashEntries = await GetUnderlyingDatabase().HashGetAsync(dataKeyName, fields);
				for (var i = 0; i < selectedPropertiesArray.Length; i++)
				{
					results[i] =
						redisHashEntries[i].IsNull
							? null
							: decoder(redisHashEntries[i], selectedPropertiesArray[i]);
				}
			}
			return results;
		}

		/// <summary>
		///     Get the next generated value for the given property
		/// </summary>
		/// <param name="property">the property for which to get the next generated value</param>
		/// <param name="incrementBy">when getting blocks of values, set this to the block size, otherwise use 1</param>
		/// <param name="sequenceName">
		///     the name under which the generated sequence is kept on the underlying database, can be null
		///     to use default name
		/// </param>
		/// <returns>The next generated value</returns>
		public virtual long GetNextGeneratedValue([NotNull] IProperty property, long incrementBy, [CanBeNull] string sequenceName)
		{
			Check.NotNull(property, "property");
			if (sequenceName == null)
			{
				sequenceName = ConstructRedisValueGeneratorKeyName(property);
			}
			// INCRBY
			return GetUnderlyingDatabase().StringIncrement(sequenceName, incrementBy);
		}

		/// <summary>
		///     Get the next generated value for the given property
		/// </summary>
		/// <param name="property">the property for which to get the next generated value</param>
		/// <param name="incrementBy">when getting blocks of values, set this to the block size, otherwise use 1</param>
		/// <param name="sequenceName">
		///     the name under which the generated sequence is kept on the underlying database, can be null
		///     to use default name
		/// </param>
		/// <param name="cancellationToken">propagates notification that operations should be canceled</param>
		/// <returns>The next generated value</returns>
		public virtual Task<long> GetNextGeneratedValueAsync(
			[NotNull] IProperty property, long incrementBy,
			[CanBeNull] string sequenceName, CancellationToken cancellationToken)
		{
			Check.NotNull(property, "property");
			cancellationToken.ThrowIfCancellationRequested();
			if (sequenceName == null)
			{
				sequenceName = ConstructRedisValueGeneratorKeyName(property);
			}
			// Async INCRBY
			return GetUnderlyingDatabase().StringIncrementAsync(sequenceName, incrementBy);
		}

		private static string EncodeKeyValue([NotNull] object propertyValue)
		{
			Check.NotNull(propertyValue, "propertyValue");
			return Escape(Convert.ToString(propertyValue));
		}

		private static string Escape(string s)
		{
			return s.Replace(KeyNameSeparator, EscapedKeyNameSeparator);
		}

		private static byte[] EncodeAsBytes(object value)
		{
			Check.NotNull(value, "value");
			return value as byte[] ??
				   Encoding.UTF8.GetBytes(Convert.ToString(value));
		}

		private static object DecodeBytes([NotNull] byte[] bytes, [NotNull] IProperty property)
		{
			Check.NotNull(bytes, "bytes");
			Check.NotNull(property, "property");
			if (property.ClrType == typeof(byte[]))
			{
				return bytes;
			}
			var value = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			if (value == null)
			{
				throw new ArgumentException(
					string.Format(
						CultureInfo.InvariantCulture,
						RedisStrings.InvalidDatabaseValue,
						"[" + string.Join(",", bytes.AsEnumerable()) + "]"));
			}
			var underlyingType = property.ClrType;
			if (typeof(string) == underlyingType)
			{
				return value;
			}
			if (typeof(Int32) == underlyingType || typeof(Int32?) == underlyingType)
			{
				return MaybeNullable(Convert.ToInt32(value), property);
			}
			if (typeof(Int64) == underlyingType || typeof(Int64?) == underlyingType)
			{
				return MaybeNullable(Convert.ToInt64(value), property);
			}
			if (typeof(Double) == underlyingType || typeof(Double?) == underlyingType)
			{
				return MaybeNullable(Convert.ToDouble(value), property);
			}
			if (typeof(Decimal) == underlyingType || typeof(Decimal?) == underlyingType)
			{
				return MaybeNullable(Convert.ToDecimal(value), property);
			}
			if (typeof(DateTime) == underlyingType || typeof(DateTime?) == underlyingType)
			{
				return MaybeNullable(DateTime.Parse(value), property);
			}
			if (typeof(DateTimeOffset) == underlyingType || typeof(DateTimeOffset?) == underlyingType)
			{
				DateTimeOffset offset;
				DateTimeOffset.TryParse(value, out offset);
				return MaybeNullable(offset, property);
			}
			if (typeof(Single) == underlyingType || typeof(Single?) == underlyingType)
			{
				return MaybeNullable(Convert.ToSingle(value), property);
			}
			if (typeof(Boolean) == underlyingType || typeof(Boolean?) == underlyingType)
			{
				return MaybeNullable(Convert.ToBoolean(value), property);
			}
			if (typeof(Byte) == underlyingType || typeof(Byte?) == underlyingType)
			{
				return MaybeNullable(Convert.ToByte(value), property);
			}
			if (typeof(UInt32) == underlyingType || typeof(UInt32?) == underlyingType)
			{
				return MaybeNullable(Convert.ToUInt32(value), property);
			}
			if (typeof(UInt64) == underlyingType || typeof(UInt64?) == underlyingType)
			{
				return MaybeNullable(Convert.ToUInt64(value), property);
			}
			if (typeof(Int16) == underlyingType || typeof(Int16?) == underlyingType)
			{
				return MaybeNullable(Convert.ToInt16(value), property);
			}
			if (typeof(UInt16) == underlyingType || typeof(UInt16?) == underlyingType)
			{
				return MaybeNullable(Convert.ToUInt16(value), property);
			}
			if (typeof(Char) == underlyingType || typeof(Char?) == underlyingType)
			{
				return MaybeNullable(Convert.ToChar(value), property);
			}
			if (typeof(SByte) == underlyingType || typeof(SByte?) == underlyingType)
			{
				return MaybeNullable(Convert.ToSByte(value), property);
			}
            if (typeof(TimeSpan) == underlyingType || typeof(TimeSpan?) == underlyingType)
            {
                return TimeSpan.Parse(value);
            }
            if (typeof(Guid) == underlyingType || typeof(Guid?) == underlyingType)
            {
                return Guid.Parse(value);
            }

            if (underlyingType.GetTypeInfo().IsEnum)
            {
                return Enum.Parse(underlyingType, value);
            }

            var nullableType = Nullable.GetUnderlyingType(underlyingType);
            if (nullableType != null && nullableType.GetTypeInfo().IsEnum)
            {
                return Enum.Parse(nullableType, value);
            }

            throw new ArgumentOutOfRangeException("property",
				string.Format(
					CultureInfo.InvariantCulture,
					RedisStrings.UnableToDecodeProperty,
					property.Name,
					underlyingType.FullName,
					property.DeclaringEntityType.Name));
		}

		private static object MaybeNullable<T>(T value, IProperty property)
			where T : struct
		{
			if (property.IsNullable)
			{
				return (T?)value;
			}
			return value;
		}

		private sealed class AsyncEnumerable : IAsyncEnumerable<object[]>
		{
			private readonly RedisStore _redisDatastore;
			private readonly RedisQuery _redisQuery;
			public AsyncEnumerable(
				RedisStore redisDatastore,
				RedisQuery redisQuery)
			{
				_redisDatastore = redisDatastore;
				_redisQuery = redisQuery;
			}

			public IAsyncEnumerator<object[]> GetEnumerator()
			{
				return new AsyncEnumerator(this);
			}

			private sealed class AsyncEnumerator : IAsyncEnumerator<object[]>
			{
				private readonly AsyncEnumerable _enumerable;
				private RedisValue[] _entityKeysForQuery;
				private int _currentOffset = -1;
				private object[] _current;
				private bool _disposed;

				public AsyncEnumerator(AsyncEnumerable enumerable)
				{
					_enumerable = enumerable;
				}

				public async Task<bool> MoveNext(CancellationToken cancellationToken)
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (_entityKeysForQuery == null)
					{
						await InitializeRedisKeys(cancellationToken);
					}
					var hasNext = (++_currentOffset < _entityKeysForQuery.Length);
					if (!hasNext)
					{
						_current = null;
						// H.A.C.K.: Workaround https://github.com/Reactive-Extensions/Rx.NET/issues/5
						Dispose();
						return false;
					}
					_current = await _enumerable._redisDatastore.GetQueryObjectsFromDatabaseAsync(
						_entityKeysForQuery[_currentOffset],
						_enumerable._redisQuery,
						DecodeBytes);
					return true;
				}

				private async Task InitializeRedisKeys(CancellationToken cancellationToken)
				{
					cancellationToken.ThrowIfCancellationRequested();
					var redisPrimaryKeyIndexKeyName =
						ConstructRedisPrimaryKeyIndexKeyName(_enumerable._redisQuery.EntityType);
					_entityKeysForQuery = await _enumerable
						._redisDatastore.GetUnderlyingDatabase().SetMembersAsync(redisPrimaryKeyIndexKeyName);
				}

				public object[] Current
				{
					get
					{
						if (_current == null)
						{
							throw new InvalidOperationException();
						}
						return _current;
					}
				}

				public void Dispose()
				{
					if (!_disposed)
					{
						_disposed = true;
					}
				}
			}
		}
	}
}
