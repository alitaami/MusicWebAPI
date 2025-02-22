using Entities.Base;
using System;

namespace MusicWebAPI.Domain.Entities
{
    public class PlaylistSong : BaseEntity<Guid>
    {
        public Guid SongId { get; set; }

        public Playlist Playlist { get; set; }
        public Song Song { get; set; }
    }
}
