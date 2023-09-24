using Notifications.ObjectModel.Models;
using Microsoft.EntityFrameworkCore;

namespace Notifications.ObjectModel;

public interface INotificationsContext
{
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationState> NotificationStates { get; set; }
}