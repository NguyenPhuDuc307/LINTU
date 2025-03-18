using LMS.Data;
using LMS.Data.Entities;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace LMS.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SendMessage(int roomId, string user, string message)
        {
            if (string.IsNullOrEmpty(message)) return; // Không gửi tin rỗng

            // Lưu tin nhắn vào database
            var newMessage = new Message
            {
                UserId = user,
                Content = message,
                ChatRoomId = roomId,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            // Gửi tin nhắn đến tất cả user trong phòng
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", user, message, DateTime.Now.ToString("HH:mm"));
        }

        public async Task JoinRoom(int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Group(roomId.ToString()).SendAsync("UserJoined", $"{Context.User!.Identity!.Name} đã tham gia phòng.");
        }

        public async Task LeaveRoom(int roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Group(roomId.ToString()).SendAsync("UserLeft", $"{Context.User!.Identity!.Name} đã rời phòng.");
        }
    }
}
