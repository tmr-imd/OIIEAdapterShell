﻿namespace TaskQueueing.ObjectModel.Enums;

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
