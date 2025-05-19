using Entities.Base;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicWebAPI.Domain.Entities
{
    public class PlaylistSong : BaseEntity<Guid>
    {
        public Guid SongId { get; set; }
        public Guid PlayListId { get; set; }

        [ForeignKey("PlayListId")]
        public Playlist Playlist { get; set; }

        [ForeignKey("SongId")]
        public Song Song { get; set; }
    }
}
