using ChatServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatServer.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string text)
        {
            var senderId = int.Parse(Context.UserIdentifier!);
            var senderName = Context.User!.Identity!.Name;

            var message = new Message
            {
                Text = text,
                SenderId = senderId,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", new
            {
                Id = message.Id,
                Text = message.Text,
                SenderId = message.SenderId,
                SenderName = senderName,
                Timestamp = message.Timestamp,
                IsPrivate = false
            });
        }

        public async Task SendPrivateMessage(int receiverId, string text)
        {
            var senderId = int.Parse(Context.UserIdentifier!);
            var senderName = Context.User!.Identity!.Name;

            var message = new Message
            {
                Text = text,
                SenderId = senderId,
                ReceiverId = receiverId,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageDto = new
            {
                Id = message.Id,
                Text = message.Text,
                SenderId = message.SenderId,
                SenderName = senderName,
                ReceiverId = receiverId,
                Timestamp = message.Timestamp,
                IsPrivate = true
            };

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", messageDto);

            if (senderId != receiverId)
            {
                await Clients.User(senderId.ToString()).SendAsync("ReceiveMessage", messageDto);
            }
        }

        public async Task DeleteMessage(int messageId)
        {
            var userId = int.Parse(Context.UserIdentifier!);

            var message = await _context.Messages.FindAsync(messageId);
            if (message == null) return;

            if (message.SenderId != userId)
            {
                throw new HubException("Запрещено удалять чужие сообщения");
            }

            message.IsDeleted = true;
            await _context.SaveChangesAsync();

            if (message.ReceiverId.HasValue)
            {
                await Clients.Users(message.SenderId.ToString(), message.ReceiverId.Value.ToString())
                             .SendAsync("MessageDeleted", messageId);
            }
            else
            {
                await Clients.All.SendAsync("MessageDeleted", messageId);
            }
        }
    }
}