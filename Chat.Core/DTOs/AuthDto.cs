using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Core.DTOs
{
    public class AuthDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
