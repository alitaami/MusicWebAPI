using Entities.Base;
using System;
using System.Collections.Generic;

namespace MusicWebAPI.Domain.Entities
{
    public class Genre : BaseEntity<Guid>
    { 
        public string Name { get; set; }
        public ICollection<Song> Songs { get; set; }
    }
}
