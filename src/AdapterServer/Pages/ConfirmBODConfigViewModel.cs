using System.ComponentModel.DataAnnotations;
using AdapterServer.Data;

namespace AdapterServer.Pages
{
    public class ConfirmBODConfigViewModel
    {
        public ConfirmationSettings Config { get; set; } = new ConfirmationSettings(new List<ConfirmBODSetting>());

        [Required(AllowEmptyStrings = false)]
        [RegularExpression(@"^((\*)|(/[^/]+(/[^/]+)*))$", ErrorMessage = "The Channel URI must be either an asterisk or an absolute URI path")]
        [Display(Name = "Channel URI")]
        public string ChannelUriToCheck { get; set; } = "";

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Topic")]
        public string TopicToCheck { get; set; } = "";

        public string CheckResult { get; set; } = "";

        public async Task Setup(SettingsService settings)
        {
            Config = await settings.LoadSettings<ConfirmationSettings>("BODConfirmations");
        }

        public async Task CheckConfirmationRequired(SettingsService settings)
        {
            // Refresh in case changes have happened elsewhere
            await Setup(settings);

            CheckResult = Config.ConfirmationOptionFor(ChannelUriToCheck, TopicToCheck).ToString();
        }
    }
}