namespace Notifications.ObjectModel;

/// <summary>
/// Type used by Notifications to indicate whether it has been read,
/// processed, or is yet to be dealt with a notification specific fashion.
/// </summary>
/// <remarks>
/// The specific states are somewhat specific to the types of notifications
/// that are being handled and possibly their interaction with their Scope.
/// Generally speaking, if a Notification is marked as UserDependent and/or
/// AppDependent then the actual (un)read states need to be determine by
/// joining on the NotificationStates table.
/// </remarks>
public enum ReadState
{
    /// <summary>
    /// Indicates that the read state is undefined. Usually an error.
    /// </summary>
    /// <remarks>
    /// Gives an explicit name to the default 'zero' value.
    /// </remarks>
    Undefined = 0,
    /// <summary>
    /// Indicates that a notification is unread or yet-to-be processed.
    /// </summary>
    /// <remarks>
    /// Generic state to be used when no user/app differentiation is required.
    /// </remarks>
    Unread = 1,
    /// <summary>
    /// Indicates that a notification has been read/processed.
    /// </summary>
    /// <remarks>
    /// Generic state to be used when no user/app differentiation is required.
    /// </remarks>
    Read = 2,
    /// <summary>
    /// Indicates that a notification's read/processed state is based on
    /// individual users.
    /// </summary>
    /// <remarks>
    /// For example, in a push notification situation, the expectation would be
    /// that each user would mark the notification read, while for an email
    /// situation the notification will be processed once an email has been
    /// sent to all relevant email addresses.
    /// </remarks>
    UserDependent = 4,
    /// <summary>
    /// Indicates that a notification's read/processed state is based on
    /// individual application instances.
    /// </summary>
    /// <remarks>
    /// For example, a notification with internal scope may have each
    /// application instance indicate when they have processed it.
    /// </remarks>
    AppDependent = 8,
    /// <summary>
    /// Indicates that a notification is both UserDependent and AppDependent.
    /// </summary>
    /// <remarks>
    /// This is a convenience constant for the bitwise OR of UserDependent and AppDependent.
    /// </remarks>
    UserAndAppDependent = UserDependent | AppDependent,
}
