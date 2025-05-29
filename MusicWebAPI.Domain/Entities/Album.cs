using Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicWebAPI.Domain.Entities
{
    public class Album : BaseEntity<Guid>
    {
        public string Title { get; set; }
        public string UserId { get; set; } // Artist ID

        public DateTime ReleaseDate { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public User Artist { get; set; }
        public ICollection<Song> Songs { get; set; }
    }
}
