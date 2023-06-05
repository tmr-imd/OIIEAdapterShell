using System.ComponentModel.DataAnnotations.Schema;

namespace CIRLib.ObjectModel.Models;
public record class Property : ModelObject
{
    public string PropertyId { get; set; } = "";
    public string PropertyValue { get; set; } = "";
    public string DataType { get; set; } = "";
    public string CategoryRefId { get; set; } = "";
    [ForeignKey("CategoryRefId")]
    public Category Category {get; set; } = null!;
    public string RegistryRefId { get; set; } = "";
    [ForeignKey("RegistryRefId")]
    public Registry Registry {get; set; } = null!;
    public string EntryRefIdInSource { get; set; } = "";
    [ForeignKey("EntryRefIdInSource")]
    public Entry Entry {get; set; } = null!;
    public ICollection<PropertyValue> PropertyValues {get; set;} = new List<PropertyValue>();

}
