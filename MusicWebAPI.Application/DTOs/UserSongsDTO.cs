using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.DTOs
{
    public class UserSongsDTO
    {
        public class AddToPlaylistDTO
        {
            public Guid SongId { get; set; }
            /// <summary>
            /// The ID of the playlist (optional). If null, <c>PlaylistName</c> will be used to create a new playlist.
            /// </summary>
            public string? PlaylistId { get; set; } = null;
            /// <summary>
            /// The name of the playlist to create (optional if <c>PlaylistId</c> is provided).
            /// </summary>
            [Description("The name of the playlist to create (optional if PlaylistId is provided).")]
            public string? PlaylistName { get; set; }
        }
    }
}
