using Common.Utilities;
using Entities.Base;
using Microsoft.EntityFrameworkCore;
using MusicWebAPI.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.Data.Context
{
    public class MusicDbContext : DbContext
    {
        public MusicDbContext(DbContextOptions<MusicDbContext> options) : base(options) { }

        // Define your DbSet<TEntity> properties here, e.g.:
        //public DbSet<Artist> Artists { get; set; }
        //public DbSet<Track> Tracks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // we need assembly of Entities Class Library, IEntity exists in that class library and we get assembly from that
            var entitiesAssembly = typeof(IEntity).Assembly;

            // register all entities to database automaticilly by Reflection
            //*** they should inherit IEntity ***///
            modelBuilder.RegisterAllEntities<IEntity>(entitiesAssembly);

            // this method is used for when we have fluent api in classes and we wanna push themm into database operations
            //*** they should inherit IEntity ***///
            modelBuilder.RegisterEntityTypeConfiguration(entitiesAssembly);

            // for cascade handlling
            modelBuilder.AddRestrictDeleteBehaviorConvention();

            //// it submits SequentialGuid  for classes,those inherits IEntity<Guid> 
            //modelBuilder.AddSequentialGuidForIdConvention();

            //// When it creates tables , it pluralize name   example =>  Class name : User , TableName : Users
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Apply singularizing or pluralizing logic here
                string tableName = entityType.GetTableName();
                entityType.SetTableName(PluralizeTableName(tableName));
            }



            // Configure your entity models here
        }

        private string PluralizeTableName(string tableName)
        {
            // Implement custom pluralization logic here (similar to the previous approach)
            if (tableName.EndsWith("y"))
                return tableName.Substring(0, tableName.Length - 1) + "ies";

            if (tableName.EndsWith("s"))
                return tableName + "es";

            return tableName + "s";
        }
        #region Save Changes Override
        public override int SaveChanges()
        {
            _cleanString();
            return base.SaveChanges();
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _cleanString();
            return await base.SaveChangesAsync(cancellationToken);
        }
        private void _cleanString()
        {
            var changedEntities = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);
            foreach (var item in changedEntities)
            {
                if (item.Entity == null)
                    continue;

                var properties = item.Entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(string));

                foreach (var property in properties)
                {
                    var propName = property.Name;
                    var val = (string)property.GetValue(item.Entity, null);

                    if (val.HasValue())
                    {
                        var newVal = val.Fa2En().FixPersianChars();
                        if (newVal == val)
                            continue;
                        property.SetValue(item.Entity, newVal, null);
                    }
                }
            }
            #endregion
        }
    }
}
