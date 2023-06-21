using System.ComponentModel.DataAnnotations.Schema;

namespace CIRLib.ObjectModel.Models;
public record class Property : ModelObject
{
    public string PropertyId { get; set; } = "";
    public string PropertyValue { get; set; } = "";
    public string DataType { get; set; } = "";
    
    public Guid EntryRefId {get; set;}
    [ForeignKey("EntryRefId")]
    public Entry Entry {get; set; } = null!;
    public string EntryIdInSource { get; set; } = "";
    
    public ICollection<PropertyValue> PropertyValues {get; set;} = new List<PropertyValue>();

}
