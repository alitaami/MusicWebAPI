using Common.Utilities;
using Entities.Base;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.Data.Context
{
    public class MusicDbContext : IdentityDbContext<User>
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
                entityType.SetTableName(Pluralize(tableName));
            }



            // Configure your entity models here
        }


        private static readonly Dictionary<string, string> IrregularPluralization = new Dictionary<string, string>
        {
            { "person", "people" },
            { "child", "children" },
            { "man", "men" },
            { "woman", "women" }
        };

        private static string Pluralize(string word)
        {
            // Check for irregular words first
            if (IrregularPluralization.ContainsKey(word.ToLower()))
            {
                return IrregularPluralization[word.ToLower()];
            }

            // Basic pluralization rules
            if (word.EndsWith("y"))
                return word.Substring(0, word.Length - 1) + "ies";

            if (word.EndsWith("s"))
                return word + "es";

            return word + "s"; // Default rule
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
