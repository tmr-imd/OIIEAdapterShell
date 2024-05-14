using AdapterServer.Data;
using AdapterServer.Services;
using Oiie.Settings;
using Hangfire;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Xml.Linq;
using TaskQueueing.Jobs;
using TaskQueueing.ObjectModel;
using TaskModels = TaskQueueing.ObjectModel.Models;
using TaskEnums = TaskQueueing.ObjectModel.Enums;

namespace AdapterServer.Pages.Request;

public abstract class AbstractRequestResponseViewModel<T> where T : struct, System.Enum
{
    public string Endpoint { get; set; } = "";
    public string ChannelUri { get; set; } = "/asset-institute/server/request-response";
    public string Topic { get; set; } = "Test Topic";
    public string SessionId { get; set; } = "";

    public T MessageType { get; set; } = Enum.GetValues<T>().First();

    public bool Ready { get; set; }

    public bool HasSession => !string.IsNullOrWhiteSpace(SessionId) ;

    protected readonly SettingsService settings;

    public AbstractRequestResponseViewModel(IOptions<ClientConfig> config, SettingsService settings)
    {
        Endpoint = config.Value?.EndPoint ?? "";
        this.settings = settings;
    }

    public async Task LoadSettings(string channelName)
    {
        try
        {
            var channelSettings = await settings.LoadSettings<ChannelSettings>(channelName);

            ChannelUri = channelSettings.ChannelUri;
            Topic = channelSettings.Topic;
            SessionId = channelSettings.ConsumerSessionId;
            MessageType = Enum.GetValues<T>().SingleOrDefault(e => e.ToString() == channelSettings.MessageType, Enum.GetValues<T>().First());
        }
        catch (FileNotFoundException)
        {
            // Just leave things as they are
        }
    }

    /// <summary>
    /// Loads the view, e.g., retrieve a list of messages of interest. Default does nothing.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public virtual async Task Load(IJobContext context) { await Task.CompletedTask; }

    /// <summary>
    /// Post a message. Default implementation does nothing.
    /// </summary>
    virtual public void Post() { }
}
