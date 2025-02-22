using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Identity;
using System.Runtime.Intrinsics.X86;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, MusicDbContext context)
    {
        try
        {
            // Check if there are existing users, and add if necessary
            if (!await context.Users.AnyAsync(u => u.Id == "0080abda-f996-42df-81a4-024fdaa47b0c"))
            {
                var user1 = new User
                {
                    Id = "0080abda-f996-42df-81a4-024fdaa47b0c",
                    UserName = "Dariush",
                    FullName = "Dariush Eghbali",
                    Bio = "Artist bio",
                    ImageUrl = "https://example.com/artist1.jpg",
                    IsArtist = true
                };
          
                // Create an instance of the PasswordHasher for your User type
                var passwordHasher = new PasswordHasher<User>();

                // Hash the password (for example "19851381 ")
                user1.PasswordHash = passwordHasher.HashPassword(user1, "19851381");

                // Optionally, set a SecurityStamp (or leave it to be generated automatically)
                user1.SecurityStamp = Guid.NewGuid().ToString();


                await context.Users.AddAsync(user1);
            }

            if (!await context.Users.AnyAsync(u => u.Id == "7525f791-f9d5-4915-817e-d658f37fc0ee"))
            {
                var user2 = new User
                {
                    Id = "7525f791-f9d5-4915-817e-d658f37fc0ee",
                    UserName = "AliTaami",
                    FullName = "Ali Taami",
                    Bio = "User bio",
                    ImageUrl = "https://example.com/artist2.jpg",
                    IsArtist = false
                };

                // Create an instance of the PasswordHasher for your User type
                var passwordHasher = new PasswordHasher<User>();

                // Hash the password (for example "19851381 ")
                user2.PasswordHash = passwordHasher.HashPassword(user2, "19851381");

                // Optionally, set a SecurityStamp (or leave it to be generated automatically)
                user2.SecurityStamp = Guid.NewGuid().ToString();

                await context.Users.AddAsync(user2);
            }

            // Check if genres already exist before inserting
            if (!await context.Genres.AnyAsync(g => g.Id == new Guid("2ec9b0e3-1e30-4498-ba25-1b3b2a0f28f0")))
            {
                var genre1 = new Genre { Id = new Guid("2ec9b0e3-1e30-4498-ba25-1b3b2a0f28f0"), Name = "Rock" };
                await context.Genres.AddAsync(genre1);
            }

            if (!await context.Genres.AnyAsync(g => g.Id == new Guid("cd94c620-fdc5-43e0-a2f9-8a026cbce18d")))
            {
                var genre2 = new Genre { Id = new Guid("cd94c620-fdc5-43e0-a2f9-8a026cbce18d"), Name = "Pop" };
                await context.Genres.AddAsync(genre2);
            }

            // Check if albums exist before inserting
            if (!await context.Albums.AnyAsync(a => a.Id == new Guid("4db1a461-b2fc-41c9-9e9f-88119d65767f")))
            {
                var album1 = new Album
                {
                    Id = new Guid("4db1a461-b2fc-41c9-9e9f-88119d65767f"),
                    Title = "Album One",
                    ReleaseDate = DateTime.UtcNow,
                    UserId = "0080abda-f996-42df-81a4-024fdaa47b0c"
                };
                await context.Albums.AddAsync(album1);
            }

            if (!await context.Albums.AnyAsync(a => a.Id == new Guid("244e0ae2-9585-489f-abff-c006544b0698")))
            {
                var album2 = new Album
                {
                    Id = new Guid("244e0ae2-9585-489f-abff-c006544b0698"),
                    Title = "Album Two",
                    ReleaseDate = DateTime.UtcNow.AddMonths(-1),
                    UserId = "0080abda-f996-42df-81a4-024fdaa47b0c"
                };
                await context.Albums.AddAsync(album2);
            }

            // Check if songs already exist before inserting
            if (!await context.Songs.AnyAsync(s => s.Title == "Song One"))
            {
                var song1 = new Song
                {
                    Title = "Song One",
                    UserId = "0080abda-f996-42df-81a4-024fdaa47b0c",
                    AlbumId = new Guid("244e0ae2-9585-489f-abff-c006544b0698"),
                    GenreId = new Guid("2ec9b0e3-1e30-4498-ba25-1b3b2a0f28f0"),
                    Duration = TimeSpan.FromMinutes(3),
                    AudioUrl = "https://example.com/song1.mp3"
                };
                await context.Songs.AddAsync(song1);
            }

            if (!await context.Songs.AnyAsync(s => s.Title == "Song Two"))
            {
                var song2 = new Song
                {
                    Title = "Song Two",
                    UserId = "0080abda-f996-42df-81a4-024fdaa47b0c",
                    AlbumId = new Guid("4db1a461-b2fc-41c9-9e9f-88119d65767f"),
                    GenreId = new Guid("cd94c620-fdc5-43e0-a2f9-8a026cbce18d"),
                    Duration = TimeSpan.FromMinutes(4),
                    AudioUrl = "https://example.com/song2.mp3"
                };
                await context.Songs.AddAsync(song2);
            }

            if (!await context.Songs.AnyAsync(s => s.Title == "Song Three"))
            {
                var song3 = new Song
                {
                    Title = "Song Three",
                    UserId = "0080abda-f996-42df-81a4-024fdaa47b0c",
                    AlbumId = new Guid("4db1a461-b2fc-41c9-9e9f-88119d65767f"),
                    GenreId = new Guid("cd94c620-fdc5-43e0-a2f9-8a026cbce18d"),
                    Duration = TimeSpan.FromMinutes(4),
                    AudioUrl = "https://example.com/song3.mp3"
                };
                await context.Songs.AddAsync(song3);
            }

            // Save changes after all the additions
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log the error here
            Log.Information("An error occurred: {ex.Message}");
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
