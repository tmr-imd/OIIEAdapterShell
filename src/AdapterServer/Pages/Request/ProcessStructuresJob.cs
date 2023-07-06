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
using AdapterServer.Data;
using Oiie.Settings;
using AdapterServer.Extensions;
using Notifications;
using System.Text.Json;

namespace AdapterServer.Pages.Request;

public class ProcessStructuresJob : ProcessRequestResponseJob<StructureAssetsFilter, RequestStructures>
{
    public ProcessStructuresJob(JobContextFactory factory, ClaimsPrincipal principal, INotificationService notifications)
        : base(factory, principal, notifications)
    {
    }

    protected override async Task<RequestStructures> process(StructureAssetsFilter filter, RequestMessage request, IJobContext context, ValidationDelegate<RequestMessage> errorCallback)
    {
        var structures = StructureAssetService.GetStructures(filter);
        return await Task.FromResult(new RequestStructures(structures));
    }

    protected override async Task<bool> process(RequestStructures content, ResponseMessage response, IJobContext context, ValidationDelegate<RequestMessage> errorCallback)
    {
        // This example does not do any special processing of the response
        // Will maybe update the data file or store entity "mappings" once the entity mapping tables are in place.
        return await Task.FromResult(true);
    }

    protected override async Task<bool> validate(StructureAssetsFilter content, RequestMessage request, IJobContext context, ValidationDelegate<RequestMessage> errorCallback)
    {
        if (!(content.FilterCode.Any() || content.FilterCondition.Any() || 
                content.FilterInspector.Any() || content.FilterLocation.Any() || 
                content.FilterOwner.Any() || content.FilterType.Any()))
        {
            var warning = new MessageError(ErrorSeverity.Warning, "No filters were provided, this may return a very large number of results.");
            errorCallback(warning, request, context);

            // Send a confirm BOD with the warnings
            var confirmBOD = createConfirmBOD(request);
            var settings = await loadSettingsAsync();
            if (settings is null) return await Task.FromResult(true);
            BackgroundJob.Enqueue<RequestProviderJob<ProcessRequestResponseJob<string, string>, string, string>>(x => x.PostResponse(settings.ProviderSessionId, request.RequestId, confirmBOD, null!));
        }

        return await Task.FromResult(true);
    }

    protected override async Task<bool> validate(RequestStructures content, ResponseMessage response, IJobContext context, ValidationDelegate<ResponseMessage> errorCallback)
    {
        // For demo purposes, treat no results as an Error
        bool success = true;

        if (!content.StructureAssets.Any())
        {
            success = false;
            var error = new MessageError(ErrorSeverity.Error, "No Structures were found!");
            errorCallback(error, response, context);

            // Is there a better way to also fail the Request?
            response.Request.Failed = true;
        }

        return await Task.FromResult(success);
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

    private BODReader? _bodReader = null;

    protected override void onDeserializationFailure(JsonDocument doc, ResponseMessage response, IJobContext context, ValidationDelegate<ResponseMessage> errorCallback)
    {
        bool success = true;
        // string content = doc.Deserialize<string>() ?? "";
        string content = response.Content.Deserialize<string>() ?? "";
        _bodReader = new BODReader(new StringReader(content), "", new BODReaderSettings() { SchemaPath = "../../Lib/CCOM.Net/XSD" });

        if (!_bodReader.IsValid)
        {
            success = false;

            var error = new MessageError(ErrorSeverity.Error, "Invalid ConfirmBOD: failed to parse.");
            errorCallback(error, response, context);

            foreach (var validationError in _bodReader.ValidationErrors)
            {
                error = validationError.ToMessageError();
                errorCallback(error, response, context);
            }
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
                x => x?.BODSuccessMessage?.WarningProcessMessage?.Select(m => m.ToMessageError(ErrorSeverity.Warning)) ?? Enumerable.Empty<MessageError>()
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

    private string createConfirmBOD(RequestMessage message)
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

        return confirmBOD.SerializeToString();
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
