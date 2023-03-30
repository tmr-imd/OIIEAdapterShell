namespace TaskQueueing.ObjectModel.Models;

public record class ChannelSetting : ModelObject
{
    public string Name { get; set; } = "";
    public string SessionId { get; set; } = "";
}
