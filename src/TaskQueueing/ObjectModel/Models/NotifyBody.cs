namespace TaskQueueing.ObjectModel.Models;

public record class NotifyBody
{
    public string[] Topics { get; set; } = Array.Empty<string>();
    public string requestMessageId { get; set; } = "";
}
