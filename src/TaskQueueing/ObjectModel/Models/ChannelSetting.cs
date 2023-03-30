namespace TaskQueueing.ObjectModel.Models;

public record class ChannelSetting : ModelObject
{
    public string Name { get; set; } = "";
    public string ConsumerSessionId { get; set; } = "";
    public string ProviderSessionId { get; set; } = "";
}
