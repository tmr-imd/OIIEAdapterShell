using System.ComponentModel.DataAnnotations.Schema;

namespace CIRLib.ObjectModel.Models;
public record class PropertyValue : ModelObject
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string UnitOfMeasure { get; set; } = "";

    [ForeignKey("Property")]
    public Guid PropertyRefId {get; set;}
    public Property Property { get; set; } = null!;
    public string PropertyId { get; set; } = "";

}
