using System.Security.Claims;
using System.Text.Json;
using Hangfire;
using TaskQueueing.Data;
using TaskQueueing.Jobs;
using TaskQueueing.Persistence;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;
using PubMessage = TaskQueueing.ObjectModel.Models.Publication;

using Oagis;
using System.Xml;
using System.Xml.Serialization;
using AdapterServer.Data;

namespace AdapterServer.Pages.Publication;

public class ProcessNewStructuresJob : ProcessPublicationJob<NewStructureAsset>
{
    public SettingsService? Settings { get; set; } = new SettingsService();

    public ProcessNewStructuresJob(JobContextFactory factory, ClaimsPrincipal principal) : base(factory, principal)
    {
    }

    protected override Task<bool> process(NewStructureAsset content, PubMessage publication, IJobContext context, ValidationDelegate<PubMessage> errorCallback)
    {
        return Task.FromResult(true);
    }

    protected override async Task<bool> validate(NewStructureAsset content, PubMessage publication, IJobContext context, ValidationDelegate<PubMessage> errorCallback)
    {
        bool success = true;

        if (String.IsNullOrWhiteSpace(content.Data.Code))
        {
            success = false;
            var error = new MessageError(ErrorSeverity.Error, "New Structures must have a 'Code' field");
            onError(error, publication, context);

            // Generate a ConfirmBOD with the errors if the validation failed
            var contentString = createConfirmBOD(publication);
            var settings = await loadSettingsAsync();
            if (settings is null) return await Task.FromResult(success);
            BackgroundJob.Enqueue<PubSubProviderJob<string>>(x => x.PostPublication(settings.ProviderSessionId, contentString, "ConfirmBOD", null!));
        }

        return await Task.FromResult(success);
    }

    protected override void onError(MessageError error, PubMessage publication, IJobContext context)
    {
        base.onError(error, publication, context);
        Console.WriteLine("Encountered an error processing publication {0}: {1}", publication.MessageId, error);
    }

    private string createConfirmBOD(PubMessage message)
    {
        var confirmBOD = new ConfirmBODType()
        {
            languageCode = "en-US",
            releaseID = "9.0",
            systemEnvironmentCode = Oagis.CodeLists.SystemEnvironmentCodeEnumerationType.Production.ToString(),
            ApplicationArea = new ApplicationAreaType()
            {
                BODID = new IdentifierType()
                {
                    Value = Guid.NewGuid().ToString()
                },
                CreationDateTime = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"),
                Sender = new SenderType()
                {
                    LogicalID = new IdentifierType()
                    {
                        Value = Guid.NewGuid().ToString() // TODO
                    }
                }
            },
            DataArea = new ConfirmBODDataAreaType()
            {
                Confirm = new ConfirmType()
                {
                    OriginalApplicationArea = new ApplicationAreaType
                    {
                        BODID = new IdentifierType { Value = message.MessageId },
                        CreationDateTime = message.DateCreated.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"),
                        Sender = new SenderType
                        {
                            LogicalID = new IdentifierType { Value = Guid.NewGuid().ToString() } // TODO
                        }
                    }
                },
                BOD = new BODType[] { new BODType() }
            },
        };

        
        if (message.MessageErrors?.All(x => x.Severity <= ErrorSeverity.Warning) ?? true)
        {
            confirmBOD.DataArea.BOD[0].BODSuccessMessage = new BODSuccessMessageType();
            if (message.MessageErrors?.Any() ?? false)
            {
                confirmBOD.DataArea.BOD[0].BODSuccessMessage.WarningProcessMessage = message.MessageErrors.Select(r => toOagisMessage(r)).ToArray();
            }
        }
        else
        {
            confirmBOD.DataArea.BOD[0].BODFailureMessage = new BODFailureMessageType()
            {
                ErrorProcessMessage = (from error in message.MessageErrors
                                       where error.Severity > ErrorSeverity.Warning
                                       select toOagisMessage(error)).ToArray(),
                WarningProcessMessage = (from warning in message.MessageErrors
                                         where warning.Severity <= ErrorSeverity.Warning
                                         select toOagisMessage(warning)).ToArray()
            };
        }

        var stream = new System.IO.StringWriter();
        var writerSettings = new XmlWriterSettings()
        {
            ConformanceLevel = ConformanceLevel.Document,
            Encoding = System.Text.Encoding.UTF8,
            Indent = true,
            OmitXmlDeclaration = true,
        };
        var writer = XmlWriter.Create(stream, writerSettings);
        new XmlSerializer(typeof(ConfirmBODType)).Serialize(writer, confirmBOD);
        return stream.ToString();
    }

    private Oagis.MessageType toOagisMessage(MessageError error)
    {
        return new MessageType
        {
            Description = new DescriptionType[]
            {
                new DescriptionType()
                {
                    languageID = "en-US",
                    Value = $"{error.Message} at Line: {error.LineNumber} Position: {error.LinePosition}"
                }
            }
        };
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