using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient.Models
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public int? ReceiverId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsPrivate { get; set; }

        public override string ToString()
        {
            string privateMark = IsPrivate ? "[Личное] " : "";
            return $"{privateMark}[{Timestamp:HH:mm}] {SenderName} (ID:{SenderId}): {Text} | MsgID:{Id}";
        }
    }
}
