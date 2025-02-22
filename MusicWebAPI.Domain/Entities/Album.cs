using Entities.Base;
using System;
using System.Collections.Generic;

namespace MusicWebAPI.Domain.Entities
{
    public class Album : BaseEntity<Guid>
    {
        public string Title { get; set; }
        public string UserId { get; set; } // Artist ID

        public DateTime ReleaseDate { get; set; }

        // Navigation Properties
        public User Artist { get; set; }
        public ICollection<Song> Songs { get; set; }
    }
}
