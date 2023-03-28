using AdapterServer.Data;

namespace AdapterServer.Pages
{
    public class EditConfirmBODSettingViewModel
    {
        public string ChannelUri { get; set; } = "*";
        public string Topic { get; set; } = "*";
        public bool RequiresConfirmation { get; set; } = false;

        public async Task Load( SettingsService settings )
        {
            try
            {
                await Task.Yield();
                // var channelSettings = await settings.LoadSettings<ChannelSettings>( channelName );

                // ChannelUri = channelSettings.ChannelUri;
                // Topic = channelSettings.Topic;
                // SessionId = channelSettings.SessionId;
            }
            catch( FileNotFoundException )
            {
                // Just leave things as they are
            }
        }

        public async Task Save( SettingsService settings )
        {
            var confirmBODSetting = new ConfirmBODSetting(ChannelUri, Topic, RequiresConfirmation);

            ConfirmationSettings confirmations;
            try
            {
                confirmations = await settings.LoadSettings<ConfirmationSettings>( "BODConfirmations" );
                confirmations = confirmations with { Settings = confirmations.Settings.Append(confirmBODSetting) };
            }
            catch( FileNotFoundException )
            {
                // Save an empty configuration if necessary
                confirmations = new ConfirmationSettings(new List<ConfirmBODSetting>());
            }

            await settings.SaveSettings( confirmations, "BODConfirmations" );
        }
    }
}
