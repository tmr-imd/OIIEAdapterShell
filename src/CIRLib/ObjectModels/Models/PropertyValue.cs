using System.ComponentModel.DataAnnotations.Schema;

namespace CIRLib.ObjectModel.Models;
public record class PropertyValue : ModelObject
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string UnitOfMeasure { get; set; } = "";

    public Guid PropertyRefId { get; set; }
    [ForeignKey("PropertyRefId")]
    public Property Property { get; set; } = null!;
    public string PropertyId { get; set; } = "";

}
