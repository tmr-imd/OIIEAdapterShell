using System.Text.Json;

namespace TaskQueueing.ObjectModel.Models;

public record class Response : AbstractMessage
{
    public string RequestId { get; set; } = "";
    public string ResponseId { get; set; } = "";
}
