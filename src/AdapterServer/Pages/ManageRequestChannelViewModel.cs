using AdapterServer.Data;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace AdapterServer.Pages
{
    public class ManageRequestChannelViewModel
    {
        public string Endpoint { get; set; } = "";

        public string ChannelUri { get; set; } = "/asset-institute/demo/request-response";
        public string Topic { get; set; } = "Test Topic";
        public string ConsumerSessionId { get; set; } = "";
        public string ProviderSessionId { get; set; } = "";

        public async Task Load( SettingsService settings, string channelName )
        {
            try
            {
                var channelSettings = await settings.LoadSettings<ChannelSettings>( channelName );

                ChannelUri = channelSettings.ChannelUri;
                Topic = channelSettings.Topic;
                ConsumerSessionId = channelSettings.ConsumerSessionId;
                ProviderSessionId = channelSettings.ProviderSessionId;
            }
            catch ( FileNotFoundException )
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
                ConsumerSessionId = ConsumerSessionId,
                ProviderSessionId = ProviderSessionId
            };

            await settings.SaveSettings( channelSettings, channelName );
        }

        public async Task OpenSession( IChannelManagement channel, IConsumerRequest consumer, IProviderRequest provider, JobContext context, SettingsService settings, string channelName )
        {
            try
            {
                await channel.GetChannel( ChannelUri );
            }
            catch ( IsbmFault ex ) when ( ex.FaultType == IsbmFaultType.ChannelFault ) 
            { 
                await channel.CreateChannel<RequestChannel>(ChannelUri, "Test");
            }

            var consumerSession = await consumer.OpenSession(ChannelUri);
            ConsumerSessionId = consumerSession.Id;

            // We're cheating for the demo
            var providerSession = await provider.OpenSession(ChannelUri, Topic);
            ProviderSessionId = providerSession.Id;

            await Save( settings, channelName );

            // Add settings to the list!
            var storedSetting = await context.ChannelSettings.Where( x => x.Name == channelName).FirstOrDefaultAsync();

            if ( storedSetting is null )
            {
                storedSetting = new ChannelSetting 
                { 
                    Name = channelName,
                    ConsumerSessionId = consumerSession.Id,
                    ProviderSessionId = providerSession.Id
                };

                await context.ChannelSettings.AddAsync( storedSetting );

                await context.SaveChangesAsync();
            }
        }

        public async Task CloseSession( IChannelManagement channel, IConsumerRequest consumer, IProviderRequest provider, JobContext context, SettingsService settings, string channelName )
        {
            try
            {
                await channel.GetChannel(ChannelUri);

                await consumer.CloseSession(ConsumerSessionId);
                await provider.CloseSession(ProviderSessionId);

            }
            catch (IsbmFault ex) when (ex.FaultType == IsbmFaultType.ChannelFault)
            {
            }

            ConsumerSessionId = "";
            ProviderSessionId = "";

            await Save(settings, channelName);

            var channelSetting = await context.ChannelSettings.Where( x => x.Name == channelName ).FirstOrDefaultAsync();

            if ( channelSetting is not null  )
            {
                context.ChannelSettings.Remove( channelSetting );

                await context.SaveChangesAsync();
            }
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

            ConsumerSessionId = "";

            await Save(settings, channelName);
        }
    }
}
