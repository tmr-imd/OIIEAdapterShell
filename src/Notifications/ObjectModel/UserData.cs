namespace Notifications.ObjectModel;

/// <summary>
/// Record class to capture JSON serialisable notification data intended for
/// User notifications.
/// </summary>
/// <remarks>
/// The EntityType and EntityId are intended to allow a URL to be constructed
/// at the front-end so that the user can navigate to the affected thing for more
/// details.
/// </remarks>
/// <param name="Summary">Short summary of the notification.</param>
/// <param name="Detail">Detailed information about the notification.</param>
/// <param name="EntityType">The fully qualified name for the type of entity associated with the notification, if any.</param>
/// <param name="EntityId">The identifier of the entity associated with the notification, if any</param>
public record class UserData(string Summary, string Detail, string? EntityType, Guid? EntityId);