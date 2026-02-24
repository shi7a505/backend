using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ChatBot
{
    public class SendChatMessageRequestDto
    {
        public string Message { get; set; } = default!;
        public Guid? ConversationId { get; set; }
    }
}
