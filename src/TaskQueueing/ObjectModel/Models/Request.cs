using System.Text.Json;

namespace TaskQueueing.ObjectModel.Models;

public record class Request : AbstractMessage
{
    public string RequestId { get; set; } = "";
    public string Topic { get; set; } = "";
    public JsonDocument? ResponseContent { get; set; }
}
