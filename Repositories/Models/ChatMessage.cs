using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class ChatMessage
{
    public int MessageId { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public string MessageText { get; set; } = null!;

    public string MessageType { get; set; } = "text";

    public DateTime? SentAt { get; set; }

    public bool? IsRead { get; set; }

    public virtual User? Sender { get; set; }

    public virtual User? Receiver { get; set; }
}
