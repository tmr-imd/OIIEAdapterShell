using System.Text.Json;

namespace TaskQueueing.ObjectModel.Models;

public record class Publication : ModelObject
{
    public string JobId { get; set; } = "";
    public JsonDocument Content { get; set; } = null!;
    public string Topic { get; set; } = "";
}
