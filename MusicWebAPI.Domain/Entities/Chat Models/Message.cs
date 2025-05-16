using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Entities.Chat_Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public int GroupId { get; set; }
        public string Content { get; set; }
        public int? ReplyToMessageId { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        [ForeignKey(nameof(GroupId))]
        public ChatGroup Group { get; set; }

        [ForeignKey(nameof(Id))]
        public Message ReplyTo { get; set; }

        [ForeignKey(nameof(SenderId))]
        public User User { get; set; }
    }
}
