using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using TaskQueueing.Data;

namespace AdapterServer.Pages.Publication;

public class PublicationViewModel : IAsyncDisposable
{
    public string Endpoint { get; set; } = "";
    public string ChannelUri { get; set; } = "/asset-institute/server/pub-sub";
    public string Topic { get; set; } = "Test Topic";
    public string SessionId { get; set; } = "";

    public string Code { get; set; } = "";
    public string Type { get; set; } = "";
    public string Location { get; set; } = "";
    public string Owner { get; set; } = "";
    public string Condition { get; set; } = "";
    public string Inspector { get; set; } = "";

    public bool Ready { get; set; }

    public IEnumerable<StructureAsset> StructureAssets { get; set; } = Enumerable.Empty<StructureAsset>();
    public CancellationTokenSource CancelTokenSource { get; } = new CancellationTokenSource();

    private readonly IChannelManagement channelManagement;
    private readonly IProviderPublication provider;
    private readonly IConsumerPublication consumer;

    private PublicationChannel publishChannel = null!;
    private PublicationProviderSession providerSession = null!;
    private PublicationConsumerSession consumerSession = null!;

    public PublicationViewModel(IOptions<ClientConfig> config, IChannelManagement channelManagement, IProviderPublication provider, IConsumerPublication consumer)
    {
        Endpoint = config.Value?.EndPoint ?? "";

        this.channelManagement = channelManagement;
        this.provider = provider;
        this.consumer = consumer;
    }

    public async ValueTask DisposeAsync() => await Teardown();

    public async Task Setup()
    {
        if (Ready) return;

        try
        {
            publishChannel = await channelManagement.CreateChannel<PublicationChannel>(ChannelUri, "Test");
        }
        catch (IsbmFault e) when (e.FaultType == IsbmFaultType.ChannelFault)
        {
            await channelManagement.DeleteChannel(ChannelUri);

            publishChannel = await channelManagement.CreateChannel<PublicationChannel>(ChannelUri, "Test");
        }

        providerSession = await provider.OpenSession(ChannelUri);
        consumerSession = await consumer.OpenSession(ChannelUri, Topic);

        Ready = true;
    }

    private async Task Teardown()
    {
        CancelTokenSource.Cancel();
        Console.WriteLine("Cancelling read poll");
        await Task.Delay(2501);
        Console.WriteLine("Wait over, closing sessions");

        if (consumer != null && consumerSession != null) await consumer.CloseSession(consumerSession.Id);
        if (provider != null && providerSession != null) await provider.CloseSession(providerSession.Id);

        try
        {
            if (publishChannel != null) await channelManagement.DeleteChannel(publishChannel.Uri);
        }
        catch (IsbmFault)
        {
            // Do nothing. Although why does this seem to be getting called so often?
        }
    }

    public void Clear()
    {
        StructureAssets = Enumerable.Empty<StructureAsset>();
    }

    public async Task Process()
    {
        var messageId = await Post();

        var newStructure = await Read();
    }

    public async Task<string> Post()
    {
        var newStructure = new NewStructureAsset("Sync", new StructureAsset(Code, Type, Location, Owner, Condition, Inspector));

        var message = await provider.PostPublication(providerSession.Id, newStructure, Topic);

        return message.Id;
    }

    public async Task<NewStructureAsset> Read()
    {
        var message = await consumer.ReadPublication(consumerSession.Id);
        if (message == null) throw new Exception("No message");
        await consumer.RemovePublication(consumerSession.Id);

        var newStructure = message.MessageContent.Deserialise<NewStructureAsset>();
        StructureAssets = StructureAssets.Append(newStructure.Data);

        return newStructure;
    }

    public record class NewStructureAsset(string Verb, StructureAsset Data);

}
