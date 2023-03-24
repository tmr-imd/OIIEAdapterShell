using AdapterServer.Data;
using Isbm2Client.Interface;
using Isbm2Client.Model;

namespace AdapterServer.Pages
{
    public class ManageRequestChannelViewModel
    {
        public string Endpoint { get; set; } = "";

        public string ChannelUri { get; set; } = "/asset-institute/demo/request-response";
        public string Topic { get; set; } = "Test Topic";
        public string SessionId { get; set; } = "";

        public async Task Load( SettingsService settings, string channelName )
        {
            try
            {
                var channelSettings = await settings.LoadSettings<ChannelSettings>( channelName );

                ChannelUri = channelSettings.ChannelUri;
                Topic = channelSettings.Topic;
                SessionId = channelSettings.SessionId;
            }
            catch( FileNotFoundException )
            {
                // Just leave things as they are
            }
        }

        public async Task Save( SettingsService settings, string channelName )
        {
            var channelSettings = new ChannelSettings
            {
                ChannelUri = ChannelUri,
                Topic = Topic,
                SessionId = SessionId
            };

            await settings.SaveSettings( channelSettings, channelName );
        }

        public async Task OpenSession( IChannelManagement channel, IConsumerRequest consumer, SettingsService settings, string channelName )
        {
            try
            {
                await channel.GetChannel( ChannelUri );
            }
            catch( IsbmFault ex ) when ( ex.FaultType == IsbmFaultType.ChannelFault ) 
            { 
                await channel.CreateChannel<RequestChannel>(ChannelUri, "Test");
            }

            var session = await consumer.OpenSession(ChannelUri);

            SessionId = session.Id;

            await Save( settings, channelName );
        }

        public async Task CloseSession( IChannelManagement channel, IConsumerRequest consumer, SettingsService settings, string channelName )
        {
            try
            {
                await channel.GetChannel(ChannelUri);

                await consumer.CloseSession(SessionId);

            }
            catch (IsbmFault ex) when (ex.FaultType == IsbmFaultType.ChannelFault)
            {
            }

            SessionId = "";

            await Save(settings, channelName);
        }

        public async Task DestroyChannel(IChannelManagement channel, SettingsService settings, string channelName)
        {
            try
            {
                await channel.GetChannel(ChannelUri);

                await channel.DeleteChannel(ChannelUri);

            }
            catch (IsbmFault ex) when (ex.FaultType == IsbmFaultType.ChannelFault)
            {
            }

            SessionId = "";

            await Save(settings, channelName);
        }
    }
}
