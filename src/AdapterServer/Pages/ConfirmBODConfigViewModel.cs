using AdapterServer.Data;

namespace AdapterServer.Pages
{
    public class ConfirmBODConfigViewModel
    {
        public ConfirmationSettings Config { get; set; } = new ConfirmationSettings(new List<ConfirmBODSetting>());

        public async Task Setup(SettingsService settings)
        {
            Config = await settings.LoadSettings<ConfirmationSettings>("BODConfirmations");
        }
    }
}