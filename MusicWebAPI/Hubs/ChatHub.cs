using Microsoft.AspNetCore.SignalR;
using MusicWebAPI.Domain.Entities.Chat_Models;
using MusicWebAPI.Infrastructure.Data.Context;
using System;
using Microsoft.EntityFrameworkCore;
using MusicWebAPI.Application.ViewModels;

public class ChatHub : Hub
{
    private readonly MusicDbContext _context;

    public ChatHub(MusicDbContext context)
    {
        _context = context;
    }

    public async Task JoinGroup(string groupName, string userId)
    {
        var group = await _context.ChatGroups.FirstOrDefaultAsync(g => g.Name == groupName);
        if (group == null)
        {
            group = new ChatGroup { Name = groupName };
            _context.ChatGroups.Add(group);
            await _context.SaveChangesAsync();
        }

        var alreadyJoined = await _context.UserGroups.AnyAsync(ug => ug.GroupId == group.Id && ug.UserId == userId);
        if (!alreadyJoined)
        {
            _context.UserGroups.Add(new UserGroup
            {
                UserId = userId,
                GroupId = group.Id,
                JoinedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserJoined", userId);
    }

    public async Task LeaveGroup(string groupName, string userId)
    {
        var group = await _context.ChatGroups.FirstOrDefaultAsync(g => g.Name == groupName);
        if (group == null) return;

        var membership = await _context.UserGroups.FirstOrDefaultAsync(ug => ug.GroupId == group.Id && ug.UserId == userId);
        if (membership != null)
        {
            _context.UserGroups.Remove(membership);
            await _context.SaveChangesAsync();
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserLeft", userId);
    }

    public async Task DeleteMessage(string groupName, int messageId)
    {
        var group = await _context.ChatGroups.FirstOrDefaultAsync(g => g.Name == groupName);
        if (group == null) return;

        var message = await _context.Messages.Where(m => m.Id == messageId && m.GroupId == group.Id).FirstOrDefaultAsync();
        var repliedMessages = await _context.Messages.Where(m => m.ReplyToMessageId == messageId).Select(r=>r.Id).ToListAsync();
        if (message != null)
        {
            message.IsDeleted = true;
            await _context.SaveChangesAsync();
            await Clients.Group(message.Group.Name).SendAsync("MessageDeleted", messageId);

            // Notify clients to refresh replies referencing the deleted message
            if (repliedMessages.Any())
            {
                await Clients.Group(message.Group.Name).SendAsync("RefreshReplies", repliedMessages);
            }
        }
    }

    public async Task SendMessage(string groupName, string senderId, string content, int? replyToMessageId = null)
    {
        Console.WriteLine("SendMessage started");
        Console.WriteLine($"GroupName: {groupName}, SenderId: {senderId}, Content: {content}, ReplyToMessageId: {replyToMessageId}");
        var parentMessage = new Message();
        var group = await _context.ChatGroups.FirstOrDefaultAsync(g => g.Name == groupName);
        if (group == null) return;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == senderId);
        if (user == null)
        {
            // Handle unknown user (optional: throw error or ignore)
            return;
        }

        var message = new Message
        {
            GroupId = group.Id,
            SenderId = senderId,
            Content = content,
            SentAt = DateTime.UtcNow,
            ReplyToMessageId = replyToMessageId
        };
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        if (replyToMessageId != null)
        {
            parentMessage = await _context.Messages.Where(m => m.Id == replyToMessageId).Include(m => m.User).FirstOrDefaultAsync();
        }

        await Clients.Group(groupName).SendAsync("ReceiveMessage", new
        {
            message.Id,
            message.Content,
            message.SenderId,
            SenderUsername = user.UserName, 
            message.SentAt,
            message.ReplyToMessageId,
            ReplyToContent = message.ReplyTo != null ? message.ReplyTo.Content : null,
            ReplytoSenderId = message.ReplyTo != null ? message.ReplyTo?.User.Id : null,
            ReplyToSenderUsername = message.ReplyTo != null ? message.ReplyTo?.User.UserName : null,
            ReplyToDeleted = message.ReplyTo?.IsDeleted != null ? message.ReplyTo.IsDeleted : false
        });
    }
    public async Task GetGroupMembers(string groupName)
    {
        var group = await _context.ChatGroups.FirstOrDefaultAsync(g => g.Name == groupName);
        if (group == null)
        {
            await Clients.Caller.SendAsync("GroupNotFound", "Group not found");
            return;
        }

        var userGroupMembers = await _context.UserGroups
            .Where(ug => ug.GroupId == group.Id)
            .Select(ug => new GroupMembersViewModel
            {
                UserId = ug.UserId,
                Username = ug.User.UserName,
                FullName = ug.User.FullName
            })
            .ToListAsync();

        await Clients.Caller.SendAsync("GroupMembers", userGroupMembers);
    }

    public async Task GetChats(string groupName, int skip = 0, int take = 50)
    {
        var group = await _context.ChatGroups
            .FirstOrDefaultAsync(g => g.Name == groupName);

        if (group == null)
        {
            await Clients.Caller.SendAsync("GroupNotFound", "Group not found");
            return;
        }

        var messages = await _context.Messages
            .Where(m => m.GroupId == group.Id && !m.IsDeleted)
            .OrderByDescending(m => m.SentAt)
            .Skip(skip)
            .Take(take)
            .Select(m => new ChatViewModel
            {
                Id = m.Id,
                Content = m.Content,
                SentAt = m.SentAt,
                SenderId = m.SenderId,
                SenderUsername = m.User.UserName,
                SenderFullName = m.User.FullName,
                ReplyToMessageId = m.ReplyToMessageId,
                ReplyToContent = m.ReplyTo != null ? m.ReplyTo.Content : null,
                ReplytoSenderId = m.ReplyTo != null ? m.ReplyTo.User.Id : null,
                ReplyToSenderUsername = m.ReplyTo != null ? m.ReplyTo.User.UserName : null,
                ReplyToDeleted = m.ReplyTo.IsDeleted  != null ? m.ReplyTo.IsDeleted : false
            })
            .ToListAsync();

        await Clients.Caller.SendAsync("ChatHistory", messages);
    }

    public async Task<string?> GetUsernameByUserId(string userId)
    {
        var username = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync();

        return username;
    }

    public async Task<List<GetChatGroupsViewModel>> GetGroups()
    {
        return await _context.ChatGroups.AsNoTracking().Select(u => new GetChatGroupsViewModel
        {
            Id = u.Id,
            Name = u.Name,
        }).ToListAsync();
    }
}
