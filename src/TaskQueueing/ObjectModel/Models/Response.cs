namespace TaskQueueing.ObjectModel.Models;

public record class Response : ModelObject
{
    public string JobId { get; set; } = "";
    public string RequestId { get; set; } = "";
    public string Content { get; set; } = "";
}
