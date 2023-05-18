namespace Notifications.ObjectModel;

/// <summary>
/// Scope to which a Notification applies, which provides a
/// coarse-grained mechanism to define how far a notification
/// will reach.
/// </summary>
public enum Scope
{
    /// <summary>
    /// Indicates that the scope is undefined. Usually an error.
    /// </summary>
    /// <remarks>
    /// Gives an explicit name to the default 'zero' value.
    /// </remarks>
    Undefined = 0,
    /// <summary>
    /// Indicates that a notification is to be local to the current application
    /// instance only.
    /// </summary>
    Local = 1,
    /// <summary>
    /// Indicates that a notification is internal to the application,
    /// including all instances, e.g., push notifications to connected users.
    /// </summary>
    Internal = 2,
    /// <summary>
    /// Indicates that a notification is to be made to an external interface,
    /// e.g., email.
    /// </summary>
    External = 4,
    /// <summary>
    /// Indicates that a notification is to be made both internally and externally.
    /// </summary>
    /// <remarks>
    /// This is a convenience constant for the bitwise OR of Internal and External.
    /// </remarks>
    InternalAndExternal = Internal | External
}
