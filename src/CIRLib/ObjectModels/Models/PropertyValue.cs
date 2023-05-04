using System.ComponentModel.DataAnnotations;

namespace CIRLib.ObjectModel.Models;
public record class PropertyValue : ModelObject
{
    [Key]
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string UnitOfMeasure { get; set; } = "";

}
