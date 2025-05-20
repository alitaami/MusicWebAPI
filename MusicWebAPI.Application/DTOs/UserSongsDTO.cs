using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.DTOs
{
   public class UserSongsDTO
    {
        public class AddToPlaylistDto
        {
            public Guid SongId { get; set; }
            public Guid UserId { get; set; }
            public Guid? PlaylistId { get; set; }
            public string? PlaylistName { get; set; } // optional if PlaylistId exists
        }
        
    }
}
