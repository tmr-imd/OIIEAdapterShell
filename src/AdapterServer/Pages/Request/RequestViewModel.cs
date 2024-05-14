using AdapterServer.Data;
using Oiie.Settings;
using Hangfire;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using TaskQueueing.Data;
using TaskQueueing.Jobs;
using TaskModels = TaskQueueing.ObjectModel.Models;

namespace AdapterServer.Pages.Request;

using RequestJobJSON = RequestConsumerJob<ProcessStructuresJob, StructureAssetsFilter, RequestStructures>;
using RequestJobBOD = RequestConsumerJob<ProcessGetShowStructuresJob, XDocument, XDocument>;

public class RequestViewModel : AbstractRequestResponseViewModel<RequestViewModel.MessageTypes>
{

    public enum MessageTypes
    {
        JSON, ExampleBOD, CCOM
    }

    public string FilterCode { get; set; } = "";
    public string FilterType { get; set; } = "";
    public string FilterLocation { get; set; } = "";
    public string FilterOwner { get; set; } = "";
    public string FilterCondition { get; set; } = "";
    public string FilterInspector { get; set; } = "";

    public RequestViewModel(IOptions<ClientConfig> config, SettingsService settings) : base(config, settings)
    {
    }

    public void Request()
    {
        switch (MessageType)
        {
            case MessageTypes.JSON:
                RequestJSON();
                break;
            case MessageTypes.ExampleBOD:
                RequestExampleBOD();
                break;
            case MessageTypes.CCOM:
                throw new Exception("Not yet implemented");
        }
    }

    public void RequestJSON()
    {
        var requestFilter = new StructureAssetsFilter(FilterCode, FilterType, FilterLocation, FilterOwner, FilterCondition, FilterInspector);

        BackgroundJob.Enqueue<RequestJobJSON>(x => x.PostRequest(SessionId, requestFilter, Topic, null!));
    }

    public void RequestExampleBOD()
    {
        var requestFilter = new StructureAssetsFilter(FilterCode, FilterType, FilterLocation, FilterOwner, FilterCondition, FilterInspector);
        var bod = requestFilter.ToGetStructureAssetsBOD();
        BackgroundJob.Enqueue<RequestJobBOD>(x => x.PostRequest(SessionId, bod, Topic, null!));
    }
}
