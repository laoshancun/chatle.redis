// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Chatle.EntityFrameworkCore.Redis.Properties;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RedisTransactionManager : IDbContextTransactionManager
    {
        private readonly bool _ignoreTransactions;

        public RedisTransactionManager([NotNull] IDbContextOptions options)
        {
            Check.NotNull(options, nameof(options));

            var optionsExtension = options.Extensions.OfType<RedisOptionsExtension>().FirstOrDefault();
            if (optionsExtension != null)
            {
                _ignoreTransactions = optionsExtension.IgnoreTransactions;
            }
        }

        public virtual IDbContextTransaction CurrentTransaction { get; private set; }

        public virtual IDbContextTransaction BeginTransaction()
        {
            if (!_ignoreTransactions)
            {
                throw new InvalidOperationException(RedisStrings.TransactionsNotSupported);
            }
            CurrentTransaction  = new RedisTransaction();
            return CurrentTransaction;
        }

        public virtual Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!_ignoreTransactions)
            {
                throw new InvalidOperationException(RedisStrings.TransactionsNotSupported);
            }

            return Task.FromResult<IDbContextTransaction>(new RedisTransaction());
        }

        public virtual void CommitTransaction()
        {
            if (!_ignoreTransactions)
            {
                throw new InvalidOperationException(RedisStrings.TransactionsNotSupported);
            }
        }

        public virtual void RollbackTransaction()
        {
            if (!_ignoreTransactions)
            {
                throw new InvalidOperationException(RedisStrings.TransactionsNotSupported);
            }
        }       
    }
}
