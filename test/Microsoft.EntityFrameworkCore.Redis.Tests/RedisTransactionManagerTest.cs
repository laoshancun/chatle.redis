// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.EntityFrameworkCore.Redis.Properties;

namespace Microsoft.EntityFrameworkCore.Redis.Tests
{
    public class RedisTransactionManagerTest
    {
        [Fact]
        public void CurrentTransaction_returns_null()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseRedisDatabase(ignoreTransactions: false);

            var transactionManager = new RedisTransactionManager(optionsBuilder.Options);

            Assert.Null(transactionManager.CurrentTransaction);
        }

        [Fact]
        public void Throws_on_BeginTransaction()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseRedisDatabase(ignoreTransactions:false);

            var transactionManager = new RedisTransactionManager(optionsBuilder.Options);

            Assert.Equal(
                RedisStrings.TransactionsNotSupported,
                Assert.Throws<InvalidOperationException>(
                    () => transactionManager.BeginTransaction()).Message);
        }

        [Fact]
        public async Task Throws_on_BeginTransactionAsync()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseRedisDatabase(ignoreTransactions:false);

            var transactionManager = new RedisTransactionManager(optionsBuilder.Options);

            Assert.Equal(
                RedisStrings.TransactionsNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await transactionManager.BeginTransactionAsync())).Message);
        }

        [Fact]
        public void Throws_on_CommitTransaction()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseRedisDatabase(ignoreTransactions:false);

            var transactionManager = new RedisTransactionManager(optionsBuilder.Options);

            Assert.Equal(
                RedisStrings.TransactionsNotSupported,
                Assert.Throws<InvalidOperationException>(
                    () => transactionManager.CommitTransaction()).Message);
        }

        [Fact]
        public void Throws_on_RollbackTransaction()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseRedisDatabase(ignoreTransactions:false);

            var transactionManager = new RedisTransactionManager(optionsBuilder.Options);

            Assert.Equal(
                RedisStrings.TransactionsNotSupported,
                Assert.Throws<InvalidOperationException>(
                    () => transactionManager.RollbackTransaction()).Message);
        }
    }
}
