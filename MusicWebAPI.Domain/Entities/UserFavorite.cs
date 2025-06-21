using Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Entities
{
    public class UserFavorite : BaseEntity<Guid>
    {
        public string UserId { get; set; }
        public Guid SongId { get; set; }

        //Navigation properties
        public virtual Song Song { get; set; }
    }
}
