using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ChatBot
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public string Sender { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; }
    }
}
