namespace TaskQueueing.ObjectModel.Models;

public record class Request : ModelObject
{
    public string JobId { get; set; } = "";
    public string SessionId { get; set; } = "";
    public string RequestId { get; set; } = "";
    public string Content { get; set; } = "";
    public bool Processed { get; set; }
}
