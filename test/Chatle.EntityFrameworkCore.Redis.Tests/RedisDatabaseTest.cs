// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore.Query;

namespace Chatle.EntityFrameworkCore.Redis.Tests
{
    public class RedisDatabaseTest
    {

        [Fact]
        public void EnsureDatabaseCreated_returns_true()
        {
            var serviceProvider = RedisTestHelpers.Instance.CreateServiceProvider();
            var model = CreateModel();
            var store = CreateStore(serviceProvider);

            Assert.True(store.EnsureDatabaseCreated(model));

            store = CreateStore(serviceProvider);

            Assert.True(store.EnsureDatabaseCreated(model));
        }

        private static IRedisDatabase CreateStore(IServiceProvider serviceProvider)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseRedisDatabase();

            return RedisTestHelpers.Instance.CreateContextServices(serviceProvider, optionsBuilder.Options).GetRequiredService<IRedisDatabase>();
        }

        [Fact]
        public async Task Save_changes_adds_new_objects_to_store()
        {
            var serviceProvider = RedisTestHelpers.Instance.CreateContextServices(CreateModel());
            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = serviceProvider.GetRequiredService<IStateManager>().GetOrCreateEntry(customer);
            entityEntry.SetEntityState(EntityState.Added);

            var RedisDatabase = serviceProvider.GetRequiredService<IRedisDatabase>();

            await RedisDatabase.SaveChangesAsync(new[] { entityEntry });
			var query = new RedisQuery(entityEntry.EntityType);

			Assert.Equal(1, RedisDatabase.Store.GetResultsEnumerable(query).Count());
            Assert.Equal(new object[] { 42, "Unikorn" }, RedisDatabase.Store.GetResultsEnumerable(query).Single());
        }

        [Fact]
        public async Task Save_changes_updates_changed_objects_in_store()
        {
            var serviceProvider = RedisTestHelpers.Instance.CreateContextServices(CreateModel());

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = serviceProvider.GetRequiredService<IStateManager>().GetOrCreateEntry(customer);
            entityEntry.SetEntityState(EntityState.Added);

            var RedisDatabase = serviceProvider.GetRequiredService<IRedisDatabase>();

            await RedisDatabase.SaveChangesAsync(new[] { entityEntry });

            customer.Name = "Unikorn, The Return";
            entityEntry.SetEntityState(EntityState.Modified);

            await RedisDatabase.SaveChangesAsync(new[] { entityEntry });
			var query = new RedisQuery(entityEntry.EntityType);
			Assert.Equal(1, RedisDatabase.Store.GetResultsEnumerable(query).Count());
            Assert.Equal(new object[] { 42, "Unikorn, The Return" }, RedisDatabase.Store.GetResultsEnumerable(query).Single());
        }

        [Fact]
        public async Task Save_changes_removes_deleted_objects_from_store()
        {
            var serviceProvider = RedisTestHelpers.Instance.CreateContextServices(CreateModel());

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = serviceProvider.GetRequiredService<IStateManager>().GetOrCreateEntry(customer);
            entityEntry.SetEntityState(EntityState.Added);

            var RedisDatabase = serviceProvider.GetRequiredService<IRedisDatabase>();

            await RedisDatabase.SaveChangesAsync(new[] { entityEntry });

            // Because the database is being used directly the entity state must be manually changed after saving.
            entityEntry.SetEntityState(EntityState.Unchanged);

            customer.Name = "Unikorn, The Return";
            entityEntry.SetEntityState(EntityState.Deleted);

            await RedisDatabase.SaveChangesAsync(new[] { entityEntry });

            Assert.Equal(0, RedisDatabase.Store.GetResultsEnumerable(new RedisQuery(entityEntry.EntityType)).Count());
        }        

        private static IModel CreateModel()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            modelBuilder.Entity<Customer>(b =>
                {
                    b.HasKey(c => c.Id);
                    b.Property(c => c.Name);
                });

            return modelBuilder.Model;
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
