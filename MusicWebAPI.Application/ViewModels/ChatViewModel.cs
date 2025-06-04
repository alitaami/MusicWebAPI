using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.ViewModels
{
    public class ChatViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public string SenderId { get; set; }
        public string? SenderAvatar { get; set; }
        public string SenderUsername { get; set; }
        public string SenderFullName { get; set; }
        public int? ReplyToMessageId { get; set; }
        public string ReplyToContent { get; set; }
        public string ReplytoSenderId { get; set; }
        public string ReplytoSenderAvatar { get; set; }
        public string ReplyToSenderUsername { get; set; }
        public bool ReplyToDeleted { get; set; }
    }

    public class GroupMembersViewModel
    {
        public string UserId { get; set; }
        public string? Avatar { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
    }

    public class GetChatGroupsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}