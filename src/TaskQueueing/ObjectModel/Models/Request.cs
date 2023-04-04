using System.Text.Json;

namespace TaskQueueing.ObjectModel.Models;

public record class Request : ModelObject
{
    public string JobId { get; set; } = "";
    public string RequestId { get; set; } = "";
    public JsonDocument Filter { get; set; } = null!;
    public JsonDocument? Content { get; set; }
    public bool Processed { get; set; }
}
