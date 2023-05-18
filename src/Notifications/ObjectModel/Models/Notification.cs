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
    /// Differentiator for notifications of different types. Allows different
    /// parts of the application to listen only for notifications of interest.
    /// </summary>
    public string Topic { get; set; } = "";

    /// <summary>
    /// Notification specific data serialized as JSON.
    /// </summary>
    public JsonDocument Data { get; set; } = null!;

    // User notifications need to be marked as read?
}
