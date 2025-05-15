using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Entities.Chat_Models
{
    public class UserGroup
    {
        public int Id { get; set; }
        public string UserId { get; set; }  
        public int GroupId { get; set; }

        public DateTime JoinedAt { get; set; }

        [ForeignKey(nameof(GroupId))]
        public ChatGroup Group { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } 
    }

}
