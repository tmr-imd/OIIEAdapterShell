namespace AdapterServer.Data
{
    public class ChannelSettings
    {
        public string ChannelUri { get; set; } = "";
        public string Topic { get; set; } = "";
        public string ConsumerSessionId { get; set; } = "";
        public string ProviderSessionId { get; set; } = "";
        public string MessageType { get; set; } = "";
    }
}
