using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.ViewModels
{
    public class HomeViewModel
    {
        public class GetSongsViewModel
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string AudioUrl { get; set; }
            public string AlbumTitle { get; set; }
            public string GenreName { get; set; }
            public string ArtistName { get; set; }
            public float? Rank { get; set; }
        } 
    }
}
