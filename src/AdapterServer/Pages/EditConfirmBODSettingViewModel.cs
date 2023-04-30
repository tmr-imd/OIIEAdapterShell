using System.ComponentModel.DataAnnotations;
using AdapterServer.Data;

namespace AdapterServer.Pages
{
    public class EditConfirmBODSettingViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [RegularExpression(@"^((\*)|(/[^/]+(/[^/]+)*))$", ErrorMessage = "The field Channel URI must be either an asterisk or an absolute URI path")]
        [Display(Name = "Channel URI")]
        public string ChannelUri { get; set; } = "*";

        [Required(AllowEmptyStrings = false)]
        public string Topic { get; set; } = "*";

        [Required]
        public ConfirmationOptions RequiresConfirmation { get; set; } = ConfirmationOptions.Never;

        public async Task Load( SettingsService settings, string id )
        {
            try
            {
                var confirmations = await settings.LoadSettings<ConfirmationSettings>( "BODConfirmations" );
                var current = (from setting in confirmations.Settings
                                where setting.GetId() == id
                                select setting).First();

                ChannelUri = current.ChannelUri;
                Topic = current.Topic;
                RequiresConfirmation = current.RequiresConfirmation;
            }
            catch( FileNotFoundException )
            {
                // Just leave things as they are
            }
            catch ( InvalidOperationException )
            {
                // Just leave things as they are
            }
        }

        public async Task Save( SettingsService settings, string id )
        {
            var confirmBODSetting = new ConfirmBODSetting(ChannelUri, Topic, RequiresConfirmation);

            // No change: shortcut
            if (confirmBODSetting.GetId() == id) return;

            ConfirmationSettings confirmations;
            try
            {
                confirmations = await settings.LoadSettings<ConfirmationSettings>( "BODConfirmations" );
                // remove the old and add the new
                confirmations = confirmations with {
                    Settings = confirmations.Settings
                                .Where( s => s.GetId() != id )
                                .Append(confirmBODSetting)
                                .OrderBy( s => s)
                };
            }
            catch( FileNotFoundException )
            {
                // Save an empty configuration if necessary
                confirmations = new ConfirmationSettings(new List<ConfirmBODSetting>());
            }

            await settings.SaveSettings( confirmations, "BODConfirmations" );
        }

        public async Task Remove( SettingsService settings, string id )
        {
            var confirmations = await settings.LoadSettings<ConfirmationSettings>( "BODConfirmations" );
            confirmations = confirmations with
            {
                Settings = from s in confirmations.Settings where s.GetId() != id select s
            };
            await settings.SaveSettings(confirmations, "BODConfirmations");
        }
    }
}
