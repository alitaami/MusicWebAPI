using Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicWebAPI.Domain.Entities
{
    public class Playlist : BaseEntity<Guid>
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public bool IsDeleted { get; set; } = false;

        [ForeignKey("UserId")]
        public User User { get; set; }
        public ICollection<PlaylistSong> PlaylistSongs { get; set; }
    }
}
