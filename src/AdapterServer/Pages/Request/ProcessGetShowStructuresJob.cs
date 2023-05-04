using System.Security.Claims;
using Hangfire;
using TaskQueueing.Data;
using TaskQueueing.Jobs;
using TaskQueueing.Persistence;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;
using RequestMessage = TaskQueueing.ObjectModel.Models.Request;
using ResponseMessage = TaskQueueing.ObjectModel.Models.Response;

using CommonBOD;
using Oagis;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using AdapterServer.Data;
using System.Text.Json;

namespace AdapterServer.Pages.Request;

public class ProcessGetShowStructuresJob : ProcessRequestResponseJob<XDocument, XDocument>
{
    private BODReader? _bodReader = null;
    private StructureAssetsFilter? _filter = null;

    public ProcessGetShowStructuresJob(JobContextFactory factory, ClaimsPrincipal principal) : base(factory, principal)
    {
    }

    protected override async Task<XDocument> process(XDocument getBod, RequestMessage request, IJobContext context, ValidationDelegate<RequestMessage> errorCallback)
    {
        if (_filter is null) throw new Exception("Unexpected null StructureAssetsFilter in process GetStructuresJob.");

        var structures = StructureAssetService.GetStructures(_filter);
        var requestStructures = new RequestStructures(structures, structures.Length);
        return await Task.FromResult(requestStructures.ToShowStructureAssetsBOD());
    }

    protected override async Task<bool> process(XDocument content, ResponseMessage response, IJobContext context, ValidationDelegate<RequestMessage> errorCallback)
    {
        // This example does not do any special processing of the response
        // Will maybe update the data file or store entity "mappings" once the entity mapping tables are in place.
        return await Task.FromResult(true);
    }

    protected override async Task<bool> validate(XDocument getBod, RequestMessage request, IJobContext context, ValidationDelegate<RequestMessage> errorCallback)
    {
        // TODO: improve support for XDocument, need to ensure that the validation is performed.
        _bodReader = new BODReader(
            new StringReader(getBod.ToString()),
            $"{Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location) ?? "."}/Data/BOD/GetStructureAssets.xsd", 
            new BODReaderSettings() { SchemaPath = BODReaderSettings.DefaultSchemaPath }
        );

        if (!_bodReader.IsValid)
        {
            handleBodValidationErrors(_bodReader, request, context, errorCallback);
            var confirmBOD = _bodReader.GenerateConfirmBOD();
            var confirmDoc = new XDocument();
            using (var writer = confirmDoc.CreateWriter()) {
                new XmlSerializer(typeof(ConfirmBODType)).Serialize(writer, confirmBOD);
            }
            var settings = await loadSettingsAsync();
            if (settings is null) throw new Exception("Unable to load settings to post ConfirmBOD response in BOD validation");

            BackgroundJob.Enqueue<RequestProviderJob<ProcessRequestResponseJob<XDocument, XDocument>, XDocument, XDocument>>(x => x.PostResponse(settings.ProviderSessionId, request.RequestId, confirmDoc, null!));
            return await Task.FromResult(false);
        }

        var bod = new GenericBodType<GetType, List<StructureAssetsFilter>>("GetStructureAssets", Ccom.Namespace.URI, nounName: "StructureAssetsFilter");
        using (var reader = getBod.CreateReader())
        {
            bod = bod.CreateSerializer().Deserialize(reader) as GenericBodType<GetType, List<StructureAssetsFilter>>;
        }
        _filter = bod?.DataArea.Noun.FirstOrDefault() ?? new StructureAssetsFilter();

        if (!(_filter.FilterCode.Any() || _filter.FilterCondition.Any() || 
                _filter.FilterInspector.Any() || _filter.FilterLocation.Any() || 
                _filter.FilterOwner.Any() || _filter.FilterType.Any()))
        {
            var warning = new MessageError(ErrorSeverity.Warning, "No filters were provided, this may return a very large number of results.");
            errorCallback(warning, request, context);

            // Send a confirm BOD with the warnings (TODO: utilise the GenerateConfirmBOD. Need to be able to attach additional Validation errors)
            var confirmBOD = createConfirmBOD(request);
            var settings = await loadSettingsAsync();
            if (settings is null) return await Task.FromResult(true);
            BackgroundJob.Enqueue<RequestProviderJob<ProcessRequestResponseJob<XDocument, XDocument>, XDocument, XDocument>>(x => x.PostResponse(settings.ProviderSessionId, request.RequestId, confirmBOD, null!));
        }

        return await Task.FromResult(true);
    }

    protected override async Task<bool> validate(XDocument content, ResponseMessage response, IJobContext context, ValidationDelegate<ResponseMessage> errorCallback)
    {
        // For demo purposes, treat no results as an Error

        // TODO: improve support for XDocument, need to ensure that the validation is performed.
        _bodReader = new BODReader(
            new StringReader(content.ToString()),
            $"{Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location) ?? "."}/Data/BOD/ShowStructureAssets.xsd", 
            new BODReaderSettings() { SchemaPath = BODReaderSettings.DefaultSchemaPath }
        );

        if (!_bodReader.IsValid)
        {
            handleBodValidationErrors(_bodReader, response, context, errorCallback);
            return await Task.FromResult(false);
        }

        if (_bodReader.SimpleName == "ConfirmBOD")
        {
            onDeserializationFailure(JsonSerializer.SerializeToDocument(content.ToString()), response, context, errorCallback);
            return await Task.FromResult(false); // No need to continue with processing
        }

        var bod = new GenericBodType<ShowType, List<RequestStructures>>("ShowStructureAssets", Ccom.Namespace.URI);
        using (var reader = content.CreateReader())
        {
            bod = bod.CreateSerializer().Deserialize(reader) as GenericBodType<ShowType, List<RequestStructures>>;
        }
        var requestStructures = bod?.DataArea.Noun.FirstOrDefault();

        if (requestStructures is null || !requestStructures.StructureAssets.Any())
        {
            var error = new MessageError(ErrorSeverity.Error, "No Structures were found!");
            errorCallback(error, response, context);

            // Is there a better way to also fail the Request?
            response.Request.Failed = true;
            return await Task.FromResult(false);
        }

        return await Task.FromResult(true);
    }

    // Simple example of overriding the error handler. Preserves default behaviour and writes to console.
    protected override void onError(MessageError error, RequestMessage request, IJobContext context)
    {
        base.onError(error, request, context);
        Console.WriteLine("Encountered an error processing request {0}: {1}", request.RequestId, error);
    }

    // Simple example of overriding the error handler. Preserves default behaviour and writes to console.
    protected override void onError(MessageError error, ResponseMessage response, IJobContext context)
    {
        base.onError(error, response, context);
        Console.WriteLine("Encountered an error processing response {0}: {1}", response.ResponseId, error);
    }

    protected override void onDeserializationFailure(JsonDocument doc, ResponseMessage response, IJobContext context, ValidationDelegate<ResponseMessage> errorCallback)
    {
        bool success = true;
        string content = response.Content.Deserialize<string>() ?? "";
        _bodReader = new BODReader(new StringReader(content), "", new BODReaderSettings() { SchemaPath = BODReaderSettings.DefaultSchemaPath });

        if (!_bodReader.IsValid)
        {
            success = false;
            handleBodValidationErrors(_bodReader, response, context, errorCallback);
        }

        if (_bodReader.Verb is not ConfirmType)
        {
            success = false;
            var error = new MessageError(ErrorSeverity.Error, $"Invalid ConfirmBOD: got {_bodReader.Verb.GetType().Name} instead.");
            errorCallback(error, response, context);
        }

        if (!success) return;

        Console.Out.WriteLine(content);

        var serializer = new XmlSerializer(typeof(BODType));
        var bodNouns = _bodReader?.Nouns.Select(x => serializer.Deserialize(x.CreateReader())).Cast<BODType>() ?? Enumerable.Empty<BODType>();
        var errors = bodNouns?.SelectMany(
            x => x?.BODFailureMessage?.ErrorProcessMessage?.Select(m => toMessageError(m, ErrorSeverity.Error)) ?? Enumerable.Empty<MessageError>()
        ) ?? Enumerable.Empty<MessageError>();
        var warnings = bodNouns?.SelectMany(
            x => x?.BODFailureMessage?.WarningProcessMessage?.Select(m => toMessageError(m, ErrorSeverity.Warning)) ?? Enumerable.Empty<MessageError>()
        ) ?? Enumerable.Empty<MessageError>();
        warnings = warnings.Concat(
            bodNouns?.SelectMany(
                x => x?.BODSuccessMessage?.WarningProcessMessage?.Select((m) => { Console.Out.WriteLine(m); return toMessageError(m, ErrorSeverity.Warning);}) ?? Enumerable.Empty<MessageError>()
            ) ?? Enumerable.Empty<MessageError>()
        );

        Console.Out.WriteLine("ConfirmBOD Response Processed");

        response.Request.MessageErrors = response.Request.MessageErrors is null ? errors.Concat(warnings).ToList() : response.Request.MessageErrors.Concat(errors).Concat(warnings).ToList();

        Console.Out.WriteLine(response.Request.MessageErrors.FirstOrDefault());
        if (errors.Any())
        {
            response.Request.Failed = true;
        }
    }

    private void handleBodValidationErrors<T>(BODReader reader, T message, IJobContext context, ValidationDelegate<T> errorCallback)
        where T : AbstractMessage
    {
        var error = new MessageError(ErrorSeverity.Error, $"Invalid BOD {reader.SimpleName}: failed to parse.");
        errorCallback(error, message, context);

        foreach (var validationError in reader.ValidationErrors)
        {
            var severity = validationError.Severity switch
            {
                System.Xml.Schema.XmlSeverityType.Error => ErrorSeverity.Error,
                System.Xml.Schema.XmlSeverityType.Warning => ErrorSeverity.Warning,
                    _ => ErrorSeverity.Error
            };
            error = new MessageError(severity, validationError.Message, validationError.LineNumber, validationError.LinePosition);
            errorCallback(error, message, context);
        }
    }

    private XDocument createConfirmBOD(RequestMessage message)
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
                        BODID = new IdentifierType { Value = message.RequestId },
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

        var doc = new XDocument();
        using (var writer = doc.CreateWriter())
        {
            new XmlSerializer(typeof(ConfirmBODType)).Serialize(writer, confirmBOD);
        }
        return doc;
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

    private MessageError toMessageError(MessageType oagisError, ErrorSeverity severity)
    {
        return new MessageError(severity, oagisError.Description.First().Value);
    }

    private SettingsService Settings = new SettingsService();

    private async Task<ChannelSettings?> loadSettingsAsync()
    {
        if (Settings is null)
        {
            Console.Out.WriteLine("Settings not loaded, no SettingsService");
            return null;
        }

        try
        {
            return await Settings.LoadSettings<ChannelSettings>("request-response");
        }
        catch (FileNotFoundException)
        {
            // Just leave things as they are
            Console.Out.WriteLine("Error: Settings not loaded");
            return null;
        }
    }
}
