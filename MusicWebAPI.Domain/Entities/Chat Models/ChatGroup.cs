using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Entities.Chat_Models
{
    public class ChatGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<UserGroup> UserGroups { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
