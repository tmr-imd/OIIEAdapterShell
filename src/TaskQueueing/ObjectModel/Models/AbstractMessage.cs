using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskQueueing.ObjectModel.Models;

/// <summary>
/// Enumeration of possible message states.
/// </summary>
/// <remarks>
/// Expected state transitions:
/// <list>
/// <item> Publication: Posted -> (if confirmation requested and indicates error) -> Error </item>
/// <item> Publication: Read -> Processing -> Processed / Error </item>
/// <item> Request: Posted -> (once recieved responses and no more expected) -> Processed </item>
/// <item> Request: Posted -> (if confirmation requested and reponse indicates error) -> Error </item>
/// <item> Request: Received -> Processing -> Processed / Error </item>
/// <item> Response: Posted </item>
/// <item> Response: Received -> Processing -> Processed / Error </item>
/// </list>
/// </remarks>
/// <remarks>
/// Note that if confirmation was requested, responding with a ConfirmBOD that indicates
/// an error occurred is considered Successful processing.
/// </remarks>
public enum MessageState
{
    /// <summary>
    /// Indicates that the message is not in a proper state. Messages in this state
    /// have likely not been initialised correctly.
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Indicates the base state of having been posted/sent.
    /// </summary>
    Posted = 1,

    /// <summary>
    /// Indicates the base state of having been received.
    /// </summary>
    Received = 2,

    /// <summary>
    /// Indicates that the message is currently being processed (or queued for processing).
    /// </summary>
    Processing = 4,
    
    /// <summary>
    /// Indicates that the message successfully completed processing
    /// </summary>
    Processed = 8, 
    
    /// <summary>
    /// Indicates that there was an error processing the message.
    /// </summary>
    Error = 16
}

public enum ErrorSeverity
{
    Warning = 5, 
    Error = 10
}

public record class MessageError(ErrorSeverity Severity, string Message = "", int LineNumber = 0, int LinePosition = 0);

public abstract record AbstractMessage : ModelObject
{
    public string JobId { get; set; } = "";
    public MessageState State { get; set; } = MessageState.Undefined;
    public string MediaType { get; set; } = System.Net.Mime.MediaTypeNames.Application.Json;
    public string? ContentEncoding { get; set; } = null;
    public JsonDocument Content { get; set; } = null!;
    public IEnumerable<MessageError>? MessageErrors { get; set; } = null;

    [NotMapped]
    public bool Posted
    {
        get
        {
            return (State & MessageState.Posted) == MessageState.Posted;
        }
    }

    [NotMapped]
    public bool Received
    {
        get
        {
            return (State & MessageState.Received) == MessageState.Received;
        }
    }

    [NotMapped]
    public bool Failed
    {
        get
        {
            return (State & MessageState.Error) == MessageState.Error;
        }
        set
        {
            State = (State & (MessageState.Posted | MessageState.Received)) | (value ? MessageState.Error : State & ~MessageState.Error);
        }
    }

    [NotMapped]
    public bool Processing
    {
        get
        {
            return (State & MessageState.Processing) == MessageState.Processing;
        }
        set
        {
            State = (State & (MessageState.Posted | MessageState.Received)) | (value ? MessageState.Processing : State & ~MessageState.Processing);
        }
    }

    [NotMapped]
    public bool Processed
    {
        get
        {
            return (State & MessageState.Processed) == MessageState.Processed;
        }
        set
        {
            State = (State & (MessageState.Posted | MessageState.Received)) | (value ? MessageState.Processed : State & ~MessageState.Processed);
        }
    }
}