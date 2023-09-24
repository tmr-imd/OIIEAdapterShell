using System.Text.Json;

namespace TaskQueueing.ObjectModel.Models;

public record class Publication : AbstractMessage, IMessage
{
    public virtual string MessageId { get; set; } = "";
    public IEnumerable<string> Topics { get; set; } = Array.Empty<string>();
}
