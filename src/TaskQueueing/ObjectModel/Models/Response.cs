using System.Text.Json;

namespace TaskQueueing.ObjectModel.Models;

public record class Response : ModelObject
{
    public string JobId { get; set; } = "";
    public string RequestId { get; set; } = "";
    public string ResponseId { get; set; } = "";
    public JsonDocument Content { get; set; } = null!;
}
