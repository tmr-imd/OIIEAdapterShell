using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;
using ModelBase.ObjectModel;
using TaskQueueing.ObjectModel.Enums;

namespace TaskQueueing.ObjectModel.Models;

public interface IMessage 
{
    public string MessageId { get; }
}

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