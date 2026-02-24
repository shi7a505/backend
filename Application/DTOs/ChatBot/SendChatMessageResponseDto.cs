using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ChatBot
{
    public class SendChatMessageResponseDto
    {
        public Guid ConversationId { get; set; }
        public string Reply { get; set; } = default!;
    }
}
