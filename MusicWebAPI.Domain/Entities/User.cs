using Microsoft.AspNetCore.Identity;
using Entities.Base;
using System;
using System.Collections.Generic;

namespace MusicWebAPI.Domain.Entities
{
    public class User : IdentityUser//, IEntity // Keep IEntity for reflection registration
    {
        public string FullName { get; set; }
        public string Bio { get; set; } = string.Empty; // Artist Bio (Optional)
        public bool? IsArtist { get; set; } = false; // True if the user is an artist  & Null if the user is SuperUser

        // Navigation Properties
        public ICollection<Playlist> Playlists { get; set; }
        public ICollection<Album> Albums { get; set; }
        public ICollection<Song> Songs { get; set; }
    }
}
