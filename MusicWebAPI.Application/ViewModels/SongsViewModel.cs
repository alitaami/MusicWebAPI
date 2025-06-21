using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.ViewModels
{
    public class SongsViewModel
    {
        public class PlaylistViewModel
        {
            public Guid PlayListId { get; set; }
            public string Name { get; set; } = null!;
            public string? UserId { get; set; }
            public Guid? CreatedByUserId { get; set; } = null;
            public List<Guid> Songs { get; set; } = new();
        }

        public class UserFavoriteViewModel
        {
            public Guid Id { get; set; }
            public string UserId { get; set; }
            public Guid SongId { get; set; }
            public string SongName { get; set; }
        }

    }
}
