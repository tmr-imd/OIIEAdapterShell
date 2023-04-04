using System.Diagnostics;

namespace TaskQueueing.ObjectModel.Models;

[DebuggerDisplay("{Id}")]
public abstract record ModelObject
{
    public Guid Id { get; init; }

    public string CreatedBy { get; set; } = "";
    public DateTime DateCreated { get; set; }

    public string ModifiedBy { get; set; } = "";
    public DateTime DateModified { get; set; }
}
