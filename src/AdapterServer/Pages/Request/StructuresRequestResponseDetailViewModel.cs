using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using Oiie.Settings;
using TaskQueueing.ObjectModel;

namespace AdapterServer.Pages.Request;

public class StructuresRequestResponseDetailViewModel : RequestResponseDetailViewModel
{
    public StructuresRequestResponseDetailViewModel(IOptions<ClientConfig> config, SettingsService settings)
        : base(config, settings)
    {}

    public override async Task Load(IJobContext context, Guid requestId)
    {
        await base.Load(context, requestId);
        RequestDetailComponentType = null;
        ResponseDetailComponentType = null;

        if (RequestRawContent.Contains("GetStructureAssets") || RequestRawContent.Contains("FilterCode"))
        {
            RequestDetailComponentType = typeof(StructuresRequestDetail);
            ResponseDetailComponentType = typeof(StructuresResponseDetail);
        }
    }
}