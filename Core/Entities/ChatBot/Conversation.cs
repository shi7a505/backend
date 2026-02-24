using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.ChatBot
{
    public  class Conversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Stored as string to match JWT ClaimTypes.NameIdentifier easily (int or guid both fit)
        public string UserId { get; set; } = default!;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
