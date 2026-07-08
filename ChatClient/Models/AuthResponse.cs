using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient.Models
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}
