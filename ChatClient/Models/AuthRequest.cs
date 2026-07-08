using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient.Models
{
    public class AuthRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
