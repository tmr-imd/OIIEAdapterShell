using ModelBase.ObjectModel;

namespace Notifications.ObjectModel.Models;

public partial record class NotificationState() : ModelObject
{
    public Guid NotificationRefId { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(NotificationRefId))]
    public Notification Notification { get; set; } = null!;

    /// <summary>
    /// The read state of the notification in context. Only Read/Unread
    /// should be used here in most cases.
    /// </summary>
    public ReadState ReadState { get; set; } = ReadState.Undefined;

    /// <summary>
    /// The user or application instance for which the state applies.
    /// </summary>
    /// <remarks>
    /// The CreatedBy/ModifiedBy are not used as they are set automatically
    /// in context and so may not correctly reflect the agent to which the
    /// notification applies.
    /// </remarks>
    public string Principal { get; set; } = "";
}