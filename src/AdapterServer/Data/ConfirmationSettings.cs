namespace AdapterServer.Data
{
    public enum ConfirmationOptions
    {
        Never, OnError, Always
    }

    public record class ConfirmBODSetting(string ChannelUri, string Topic, ConfirmationOptions RequiresConfirmation);
    public record class ConfirmationSettings(IEnumerable<ConfirmBODSetting> Settings);
}