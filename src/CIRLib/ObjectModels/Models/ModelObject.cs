using System.Diagnostics;

namespace CIRLib.ObjectModel.Models;

[DebuggerDisplay("{Id}")]
public abstract record ModelObject
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public string CreatedBy { get; set; } = "authUser";
    public DateTime DateCreated { get; set; }

    public string ModifiedBy { get; set; } = "authUser";
    public DateTime DateModified { get; set; }
}
