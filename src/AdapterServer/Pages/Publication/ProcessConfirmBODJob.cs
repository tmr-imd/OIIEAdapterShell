using System.Security.Claims;
using TaskQueueing.Jobs;
using TaskQueueing.Persistence;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;
using PubMessage = TaskQueueing.ObjectModel.Models.Publication;

using CommonBOD;
using Oagis;
using System.Xml.Serialization;
using AdapterServer.Data;
using Oiie.Settings;
using AdapterServer.Extensions;

namespace AdapterServer.Pages.Publication;

public class ProcessConfirmBODJob : ProcessPublicationJob<string>
{
    private BODReader? _bodReader = null;

    public SettingsService? Settings { get; set; } = new SettingsService();

    public ProcessConfirmBODJob(JobContextFactory factory, ClaimsPrincipal principal) : base(factory, principal)
    {
    }

    protected override Task<bool> process(string content, PubMessage publication, IJobContext context, ValidationDelegate<PubMessage> errorCallback)
    {
        var serializer = new XmlSerializer(typeof(BODType));
        var bodNouns = _bodReader?.Nouns.Select(x => serializer.Deserialize(x.CreateReader())).Cast<BODType>() ?? Enumerable.Empty<BODType>();
        var errors = bodNouns?.SelectMany(
            x => x?.BODFailureMessage?.ErrorProcessMessage?.Select(m => m.ToMessageError()) ?? Enumerable.Empty<MessageError>()
        ) ?? Enumerable.Empty<MessageError>();

        var originalMessageId = (_bodReader?.Verb as ConfirmType)?.OriginalApplicationArea?.BODID?.Value ?? "";

        var originalPublication = context.Publications
            .WherePosted()
            .Where(x => x.MessageId == originalMessageId)
            .FirstOrDefault();

        if (originalPublication is not null)
        {
            originalPublication.MessageErrors = originalPublication.MessageErrors is null ? errors : originalPublication.MessageErrors.Concat(errors);
            originalPublication.Failed = true;
        }

        return Task.FromResult(true);
    }

    protected override async Task<bool> validate(string content, PubMessage publication, IJobContext context, ValidationDelegate<PubMessage> errorCallback)
    {
        var success = true;

        _bodReader = new BODReader(new StringReader(content), "", new BODReaderSettings() { SchemaPath = BODReaderSettings.DefaultSchemaPath });

        if (!_bodReader.IsValid)
        {
            success = false;

            var error = new MessageError(ErrorSeverity.Error, "Invalid ConfirmBOD: failed to parse.");
            errorCallback(error, publication, context);

            foreach (var validationError in _bodReader.ValidationErrors)
            {
                error = validationError.ToMessageError();
                errorCallback(error, publication, context);
            }
        }

        if (_bodReader.Verb is not ConfirmType)
        {
            success = false;
            var error = new MessageError(ErrorSeverity.Error, $"Invalid ConfirmBOD: got {_bodReader.Verb.GetType().Name} instead.");
            errorCallback(error, publication, context);
        }

        return await Task.FromResult(success);
    }

    protected override void onError(MessageError error, PubMessage publication, IJobContext context)
    {
        base.onError(error, publication, context);
        Console.WriteLine("Encountered an error processing publication {0}: {1}", publication.MessageId, error);
    }

    private async Task<ChannelSettings?> loadSettingsAsync()
    {
        if (Settings is null)
        {
            Console.Out.WriteLine("Settings not loaded, no SettingsService");
            return null;
        }

        try
        {
            return await Settings.LoadSettings<ChannelSettings>("pub-sub");
        }
        catch (FileNotFoundException)
        {
            // Just leave things as they are
            Console.Out.WriteLine("Error: Settings not loaded");
            return null;
        }
    }
}