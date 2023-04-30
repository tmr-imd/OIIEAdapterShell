namespace AdapterServer.Data
{
    public enum ConfirmationOptions
    {
        Never, OnError, Always
    }

    public record class ConfirmBODSetting(string ChannelUri, string Topic, ConfirmationOptions RequiresConfirmation)
        : IComparable<ConfirmBODSetting>
    {
        /// <summary>
        /// Orders least specific rule to most specific, where asterisk means "any"
        /// and "any" channel with a specific topic is less specific than a specific
        /// channel with "any" topic.
        /// </summary>
        /// <remarks>
        /// Since there may be many more channels than the known types of information,
        /// i.e., Topics/BODs, it seems that the "any" channel/specific topic combination
        /// is naturally less specific than a specific channel/"any" topic  rule.
        /// </remarks>
        /// <param name="other">The other rule to compare against</param>
        /// <returns>-1 if less specific, 0 if equal, 1 if more specific than other</returns>
        public int CompareTo(ConfirmBODSetting? other)
        {
            if (other is null) return -1;
            return ChannelUri switch
            {
                "*" when (other?.ChannelUri != "*") => -1,
                var uri when (uri == other?.ChannelUri) => Topic == "*" ? -1 : Topic.CompareTo(other?.Topic),
                var uri when (other?.ChannelUri == "*") => 1,
                var uri when (other?.ChannelUri != "*") => uri.CompareTo(other?.ChannelUri),
                _ => 0
            };
        }

        public string GetId()
        {
            return GetHashCode().ToString();
        }
    }

    public record class ConfirmationSettings(IEnumerable<ConfirmBODSetting> Settings)
    {
        /// <summary>
        /// Returns the applicable ConfirmationOptions value for the most specific
        /// request confirmation rule compared to the given uri and topic.
        /// </summary>
        /// <param name="uri">The Channel URI to compare against the rules</param>
        /// <param name="topic">The Topic to compare against the rules</param>
        /// <returns></returns>
        public ConfirmationOptions ConfirmationOptionFor(string uri, string topic)
        {
            // Order by decreasing specificity, then
            return Settings.OrderByDescending(s => s).First((s) => {
                // find the first exact match, or topic match (any channel),
                // or channel match (any topic), or any channel any topic
                return s.ChannelUri == uri && s.Topic == topic ||
                        s.ChannelUri == "*" && s.Topic == topic ||
                        s.ChannelUri == uri && s.Topic == "*" ||
                        s.ChannelUri ==  "*" && s.Topic == "*";
            }).RequiresConfirmation;
        }
    }
}