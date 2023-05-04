using System.Text.Json;

namespace TaskQueueing.ObjectModel.Models;

public record class Publication : AbstractMessage
{
    public string MessageId { get; set; } = "";
    public IEnumerable<string> Topics { get; set; } = Array.Empty<string>();
}
