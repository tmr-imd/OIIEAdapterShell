using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using Oiie.Settings;
using TaskQueueing.ObjectModel;

namespace AdapterServer.Pages.Publication;

public class StructuresPublicationDetailViewModel : PublicationDetailViewModel
{
    public StructuresPublicationDetailViewModel(IOptions<ClientConfig> config, SettingsService settings)
        : base(config, settings)
    {}

    public override async Task Load(IJobContext context, Guid messageId)
    {
        await base.Load(context, messageId);
        if (RawContent?.Contains("SyncStructureAssets") ?? false)
        {
            DetailComponentType = typeof(StructuresPublicationDetail);
        }
        else{
            DetailComponentType = null;
        }
    }
}

