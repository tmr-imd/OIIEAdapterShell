namespace AdapterServer.Data
{
    public record class ConfirmBODSetting(string ChannelUri, string Topic, bool RequiresConfirmation);
    public record class ConfirmationSettings(IEnumerable<ConfirmBODSetting> Settings);
}