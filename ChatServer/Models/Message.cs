using System;

namespace ChatServer.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;


        // Связь с отправителем
        public int SenderId { get; set; }
        public User? Sender { get; set; }

        // Связь с получателем
        // null означает что сообщение для общего чата
        public int? ReceiverId { get; set; }
        public User? Receiver { get; set; }


        // Флаг удаления
        public bool IsDeleted { get; set; } = false;
    }
}