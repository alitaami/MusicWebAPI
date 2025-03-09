using Microsoft.AspNetCore.Identity;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Infrastructure.Data.Context;
using System;
using System.Linq;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider, MusicDbContext context)
    {
        try
        {
            // Create a new instance of PasswordHasher
            var passwordHasher = new PasswordHasher<User>();

            // Check if user "Dariush" already exists
            if (!context.Users.Any(u => u.UserName == "Dariush"))
            {
                // Create user object
                var dariush = new User
                {
                    UserName = "Dariush",
                    FullName = "Dariush Eghbali",
                    Email = "dariush@gmail.com",
                    NormalizedEmail = "DARIUSH@GMAIL.COM",
                    NormalizedUserName = "DARIUSH",
                    Bio = "Artist bio",
                    IsArtist = true,
                    SecurityStamp = Guid.NewGuid().ToString(),  // Required field for Identity
                    ConcurrencyStamp = Guid.NewGuid().ToString()  // Optional field for concurrency
                };

                // Hash the password manually
                dariush.PasswordHash = passwordHasher.HashPassword(dariush, "19851381");

                // Add user to the context
                context.Users.Add(dariush);
            }

            // Check if user "AliTaami" already exists
            if (!context.Users.Any(u => u.UserName == "AliTaami"))
            {
                // Create user object
                var ali = new User
                {
                    UserName = "AliTaami",
                    NormalizedUserName = "ALITAAMI",
                    FullName = "Ali Taami",
                    Email = "alitaami@gmail.com",
                    Bio = "User bio",
                    IsArtist = false,
                    NormalizedEmail = "ALITAAMI@GMAIL.COM",
                    SecurityStamp = Guid.NewGuid().ToString(),  // Required field for Identity
                    ConcurrencyStamp = Guid.NewGuid().ToString()  // Optional field for concurrency
                };

                // Hash the password manually
                ali.PasswordHash = passwordHasher.HashPassword(ali, "19851381");

                // Add user to the context
                context.Users.Add(ali);
            }

            // Save changes to the database
            context.SaveChanges();

            // Seed roles
            if (!context.Roles.Any(r => r.Name == "Artist"))
            {
                context.Roles.Add(new IdentityRole { Name = "Artist", NormalizedName = "ARTIST" });
            }

            if (!context.Roles.Any(r => r.Name == "User"))
            {
                context.Roles.Add(new IdentityRole { Name = "User", NormalizedName = "USER" });
            }

            context.SaveChanges();

            // Assign roles to users
            var user1 = context.Users.FirstOrDefault(u => u.UserName == "Dariush");
            var user2 = context.Users.FirstOrDefault(u => u.UserName == "AliTaami");

            var artistRole = context.Roles.FirstOrDefault(r => r.Name == "Artist");
            var userRole = context.Roles.FirstOrDefault(r => r.Name == "User");

            if (user1 != null && artistRole != null)
            {
                context.UserRoles.Add(new IdentityUserRole<string> { UserId = user1.Id, RoleId = artistRole.Id });
            }

            if (user2 != null && userRole != null)
            {
                context.UserRoles.Add(new IdentityUserRole<string> { UserId = user2.Id, RoleId = userRole.Id });
            }

            context.SaveChanges();

            // Seed genres
            if (context.Database.CanConnect() && !context.Genres.Any())
            {
                context.Genres.AddRange(
                    new Genre { Id = Guid.NewGuid(), Name = "Rock" },
                    new Genre { Id = Guid.NewGuid(), Name = "Pop" }
                );
                context.SaveChanges();
            }

            // Seed albums
            var user1Entity = context.Users.FirstOrDefault(u => u.UserName == "Dariush");
            if (user1Entity != null)
            {
                if (!context.Albums.Any(a => a.Title == "Album One"))
                {
                    var album1 = new Album
                    {
                        Id = Guid.NewGuid(),
                        Title = "Album One",
                        ReleaseDate = DateTime.UtcNow,
                        UserId = user1Entity.Id
                    };
                    context.Albums.Add(album1);
                }

                if (!context.Albums.Any(a => a.Title == "Album Two"))
                {
                    var album2 = new Album
                    {
                        Id = Guid.NewGuid(),
                        Title = "Album Two",
                        ReleaseDate = DateTime.UtcNow.AddMonths(-1),
                        UserId = user1Entity.Id
                    };
                    context.Albums.Add(album2);
                }

                context.SaveChanges();
            }

            var rockGenre = context.Genres.FirstOrDefault(g => g.Name == "Rock");
            var popGenre = context.Genres.FirstOrDefault(g => g.Name == "Pop");

            // Seed songs
            if (!context.Songs.Any(s => s.Title == "Song One"))
            {
                var song1 = new Song
                {
                    Title = "Song One",
                    UserId = user1Entity?.Id,
                    AlbumId = context.Albums.FirstOrDefault(a => a.Title == "Album Two")?.Id ?? Guid.NewGuid(),
                    GenreId = rockGenre?.Id ?? Guid.NewGuid(), // Rock
                    Duration = TimeSpan.FromMinutes(3),
                    AudioUrl = "https://example.com/song1.mp3"
                };
                context.Songs.Add(song1);
            }

            if (!context.Songs.Any(s => s.Title == "Song Two"))
            {
                var song2 = new Song
                {
                    Title = "Song Two",
                    UserId = user1Entity?.Id,
                    AlbumId = context.Albums.FirstOrDefault(a => a.Title == "Album One")?.Id ?? Guid.NewGuid(),
                    GenreId = popGenre?.Id ?? Guid.NewGuid(), // Pop
                    Duration = TimeSpan.FromMinutes(4),
                    AudioUrl = "https://example.com/song2.mp3"
                };
                context.Songs.Add(song2);
            }

            if (!context.Songs.Any(s => s.Title == "Song Three"))
            {
                var song3 = new Song
                {
                    Title = "Song Three",
                    UserId = user1Entity?.Id,
                    AlbumId = context.Albums.FirstOrDefault(a => a.Title == "Album One")?.Id ?? Guid.NewGuid(),
                    GenreId = popGenre?.Id ?? Guid.NewGuid(), // Pop
                    Duration = TimeSpan.FromMinutes(4),
                    AudioUrl = "https://example.com/song3.mp3"
                };
                context.Songs.Add(song3);
            }

            // Save all changes
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
