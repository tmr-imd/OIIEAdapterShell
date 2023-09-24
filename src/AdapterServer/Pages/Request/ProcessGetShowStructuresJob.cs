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
using System.Xml.Linq;
using System.Xml.Serialization;
using AdapterServer.Data;
using Oiie.Settings;
using AdapterServer.Extensions;
using System.Text.Json;
using Transformation;
using Transformation.Extensions;
using Notifications;

namespace AdapterServer.Pages.Request;

public class ProcessGetShowStructuresJob : ProcessRequestResponseJob<XDocument, XDocument>
{
    private BODReader? _bodReader = null;
    private StructureAssetsFilter? _filter = null;

    public ProcessGetShowStructuresJob(JobContextFactory factory, ClaimsPrincipal principal, INotificationService notifications)
        : base(factory, principal, notifications)
    {
    }

    protected override Task<XDocument> process(XDocument getBod, RequestMessage request, IJobContext context, ValidationDelegate<RequestMessage> errorCallback)
    {
        if (_filter is null) throw new Exception("Unexpected null StructureAssetsFilter in process GetStructuresJob.");

        var assets = StructureAssetService.GetStructures(_filter).Select(x => {
            var converter = TypeConverterSelector.SelectConverter(x, typeof(Ccom.Asset));
            var asset = converter.ConvertTo( x, typeof(Ccom.Asset) );

            if ( asset is null )
            {
                throw new InvalidOperationException("Problem converting StructureAsset to Ccom.Asset");
            }

            return (Ccom.Asset)asset;
        })
        .ToList();

        return Task.FromResult( assets.ToShowStructureAssetsBOD() );
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
            var confirmDoc = _bodReader.GenerateConfirmBOD().SerializeToDocument();
            await postConfirmBODResponse(request, confirmDoc);
            return await Task.FromResult(false);
        }

        var bod = _bodReader.AsBod<GenericBodType<GetType, List<StructureAssetsFilter>>>();
        _filter = bod?.DataArea.Noun.FirstOrDefault() ?? new StructureAssetsFilter();

        await validateFilter(_filter, request, context, errorCallback);

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

        var bod = _bodReader.AsBod<GenericBodType<ShowType, List<Ccom.Asset>>>();
        var assets = bod?.DataArea.Noun;

        return await validateStructures(assets ?? new List<Ccom.Asset>(), response, context, errorCallback);
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

        var bod = _bodReader.AsBod<ConfirmBODType>();
        if (bod is null)
        {
            success = false;
            var error = new MessageError(ErrorSeverity.Error, $"Invalid ConfirmBOD: got {_bodReader.Verb.GetType().Name} instead.");
            errorCallback(error, response, context);
        }

        if (!success) return;

        Console.Out.WriteLine(content);

        var errors = bod?.DataArea.BOD.SelectMany(
            x => x?.BODFailureMessage?.ErrorProcessMessage?.Select(m => m.ToMessageError()) ?? Enumerable.Empty<MessageError>()
        ) ?? Enumerable.Empty<MessageError>();

        var warnings = bod?.DataArea.BOD.SelectMany(
            x => x?.BODFailureMessage?.WarningProcessMessage?.Select(m => m.ToMessageError(ErrorSeverity.Warning)) ?? Enumerable.Empty<MessageError>()
        ) ?? Enumerable.Empty<MessageError>();

        warnings = warnings.Concat(
            bod?.DataArea.BOD.SelectMany(
                x => x.BODSuccessMessage.WarningProcessMessage.Select(m => m.ToMessageError(ErrorSeverity.Warning)) ?? Enumerable.Empty<MessageError>()
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
            error = validationError.ToMessageError();
            errorCallback(error, message, context);
        }
    }

    private async Task<bool> validateStructures(List<Ccom.Asset> assets, ResponseMessage response, IJobContext context, ValidationDelegate<ResponseMessage> errorCallback)
    {
        if (assets.Any()) return await Task.FromResult(true);

        var error = new MessageError(ErrorSeverity.Error, "No Structures were found!");
        errorCallback(error, response, context);

        // Is there a better way to also fail the Request?
        response.Request.Failed = true;
        return await Task.FromResult(false);
    }

    private async Task validateFilter(StructureAssetsFilter filter, RequestMessage request, IJobContext context,ValidationDelegate<RequestMessage> errorCallback)
    {
        if (filter.FilterCode.Any() || filter.FilterCondition.Any() ||
                filter.FilterInspector.Any() || filter.FilterLocation.Any() ||
                filter.FilterOwner.Any() || filter.FilterType.Any())
        {
            return;
        }

        var warning = new MessageError(ErrorSeverity.Warning, "No filters were provided, this may return a very large number of results.");
        errorCallback(warning, request, context);

        // Send a confirm BOD with the warnings (TODO: utilise the GenerateConfirmBOD. Need to be able to attach additional Validation errors)
        var confirmBOD = createConfirmBOD(request);
        await postConfirmBODResponse(request, confirmBOD);
    }

    private async Task postConfirmBODResponse(RequestMessage request, XDocument confirmBOD)
    {
        var settings = await loadSettingsAsync();
        if (settings is null) throw new Exception("Unable to load settings to post ConfirmBOD response in BOD validation");
        
        BackgroundJob.Enqueue<RequestProviderJob<ProcessRequestResponseJob<XDocument, XDocument>, XDocument, XDocument>>(x => x.PostResponse(settings.ProviderSessionId, request.RequestId, confirmBOD, null!));
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
                CreationDateTime = DateTime.UtcNow.ToXsDateTimeString(),
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
                        CreationDateTime = message.DateCreated.ToUniversalTime().ToXsDateTimeString(),
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
                confirmBOD.DataArea.BOD[0].BODSuccessMessage.WarningProcessMessage = message.MessageErrors.Select(r => r.ToOagisMessage()).ToArray();
            }
        }
        else
        {
            confirmBOD.DataArea.BOD[0].BODFailureMessage = new BODFailureMessageType()
            {
                ErrorProcessMessage = (from error in message.MessageErrors
                                       where error.Severity > ErrorSeverity.Warning
                                       select error.ToOagisMessage()).ToArray(),
                WarningProcessMessage = (from warning in message.MessageErrors
                                         where warning.Severity <= ErrorSeverity.Warning
                                         select warning.ToOagisMessage()).ToArray()
            };
        }

        return confirmBOD.SerializeToDocument();
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
