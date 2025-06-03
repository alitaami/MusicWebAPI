using Entities.Base;
using NpgsqlTypes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicWebAPI.Domain.Entities
{ 
    public class Song : BaseEntity<Guid>
    {
        public string Title { get; set; }
        public string UserId { get; set; } // Artist ID (string)
        public Guid? AlbumId { get; set; }
        public Guid? GenreId { get; set; }
        public TimeSpan Duration { get; set; }
        public string? AudioUrl { get; set; } = null;
        public long Listens { get; set; } = 0;
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        [ForeignKey("UserId")]
        public User? Artist { get; set; }
        [ForeignKey("AlbumId")]
        public Album? Album { get; set; }
        [ForeignKey("GenreId")]
        public Genre? Genre { get; set; }

        // Full-Text Search Field
        public NpgsqlTsVector? SearchVector { get; set; } 
    }
}
