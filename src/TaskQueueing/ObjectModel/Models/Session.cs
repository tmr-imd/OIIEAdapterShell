using ModelBase.ObjectModel;

namespace TaskQueueing.ObjectModel.Models;

// JobName is scheduled task to trigger...
public record class Session( string SessionId, string RecurringJobId ) : ModelObject;
