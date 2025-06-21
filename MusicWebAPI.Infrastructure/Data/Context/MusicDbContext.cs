using Common.Utilities;
using Entities.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Entities.Chat_Models;
using MusicWebAPI.Domain.Entities.Subscription_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.Data.Context
{
    public class MusicDbContext : IdentityDbContext<User>
    {
        public MusicDbContext(DbContextOptions<MusicDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }
        public DbSet<ChatGroup> ChatGroups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Reflection
            // Get the assembly containing IEntity
            //  var entitiesAssembly = typeof(IEntity).Assembly;

            // Comment reason : because when we wanna register them by reflection, we can not have access to entities from SeedData.cs !

            // Automatically register all entities and configurations
            //modelBuilder.RegisterAllEntities<IEntity>(entitiesAssembly);
            //modelBuilder.RegisterEntityTypeConfiguration(entitiesAssembly);
            //modelBuilder.AddRestrictDeleteBehaviorConvention();

            // Pluralize table names
            //foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            //{
            //    string tableName = entityType.GetTableName();
            //    entityType.SetTableName(Pluralize(tableName));
            //}
            #endregion

            modelBuilder.HasDefaultSchema("public"); // Ensures it works with PostgreSQL

            // Optional: Rename Identity table names (e.g., "AspNetUsers" -> "Users")
            modelBuilder.Entity<User>().ToTable("Users", "public");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles", "public");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", "public");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims", "public");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins", "public");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens", "public");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims", "public");

            // Map relationships for IdentityUser (User)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Albums)
                .WithOne(a => a.Artist)
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Songs)
                .WithOne(s => s.Artist)
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<Song>()
                .HasOne(s => s.Album)
                .WithMany(a => a.Songs)
                .HasForeignKey(s => s.AlbumId);

            modelBuilder.Entity<Song>()
                .HasOne(s => s.Genre)
                .WithMany(g => g.Songs)
                .HasForeignKey(s => s.GenreId);

            modelBuilder.Entity<Song>()
                .HasGeneratedTsVectorColumn(
                    p => p.SearchVector,
                    "english",
                    p => new { p.Title/* , AlbumTitle = p.Album.Title, p.Artist.FullName, p.Genre.Name*/ })
                .HasIndex(p => p.SearchVector)
                .HasMethod("GIN");

            modelBuilder.Entity<Message>()
           .HasOne(m => m.ReplyTo)
           .WithMany()
           .HasForeignKey(m => m.ReplyToMessageId)
           .OnDelete(DeleteBehavior.Restrict);
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
            if (IrregularPluralization.ContainsKey(word.ToLower()))
            {
                return IrregularPluralization[word.ToLower()];
            }

            if (word.EndsWith("y"))
                return word.Substring(0, word.Length - 1) + "ies";

            if (word.EndsWith("s"))
                return word;

            return word + "s";
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
                    var val = (string)property.GetValue(item.Entity, null);
                    if (val.HasValue())
                    {
                        var newVal = val.Fa2En().FixPersianChars();
                        if (newVal != val)
                        {
                            property.SetValue(item.Entity, newVal, null);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
