using TaskQueueing.ObjectModel.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Notifications.ObjectModel.Models;

public partial record class Notification : ModelObject
{
    /// <summary>
    /// How far the notification should extend inside/outside the application.
    /// </summary>
    public Scope Scope { get; set; } = Scope.Undefined;

    /// <summary>
    /// Whether the notification has been read and/or processed, as appropriate.
    /// </summary>
    /// <remarks>
    /// The specific states are somewhat specific to the types of notifications
    /// that are being handled and possibly their interaction with their Scope.
    /// Generally speaking, if a Notification is marked as UserDependent and/or
    /// AppDependent then the actual (un)read states need to be determined by
    /// joining on the NotificationStates table.
    /// </remarks>
    public ReadState ReadState { get; set; }

    /// <summary>
    /// Differentiator for notifications of different types. Allows different
    /// parts of the application to listen only for notifications of interest.
    /// </summary>
    public string Topic { get; set; } = "";

    /// <summary>
    /// Notification specific data serialized as JSON.
    /// </summary>
    public JsonDocument Data { get; set; } = null!;

    /// <summary>
    /// The associated NotificationStates when ReadState is UserDependent
    /// and/or AppDependent.
    /// </summary>
    public ICollection<NotificationState> NotificationStates { get; } = new List<NotificationState>();

    // Exclude the NotificationStates inverse collection from the equality check and hashcode
    // The Data comparison is not very efficient due to needing to convert to string.
    // Even though the JsonElement is a value type, because it refers back to the JsonDocument
    // the standard equality breaks on the unmatching reference.
    public virtual bool Equals(Notification? other)
    {
        return base.Equals(other) &&
            EqualityComparer<Scope>.Default.Equals(Scope, other.Scope) &&
            EqualityComparer<ReadState>.Default.Equals(ReadState, other.ReadState) &&
            EqualityComparer<string>.Default.Equals(Topic, other.Topic) &&
            EqualityComparer<string>.Default.Equals(Data.ToString(), other.Data.ToString());
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(),
            EqualityComparer<Scope>.Default.GetHashCode(Scope),
            EqualityComparer<ReadState>.Default.GetHashCode(ReadState),
            EqualityComparer<string>.Default.GetHashCode(Topic),
            EqualityComparer<string>.Default.GetHashCode(Data.ToString() ?? ""));
    }
}
