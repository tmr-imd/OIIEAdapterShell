using System.ComponentModel.DataAnnotations.Schema;

namespace CIRLib.ObjectModel.Models;
public record class Category : ModelObject
{
    public string CategoryId { get; set; } = "";
    public string RegistryRefId { get; set; } = "";
    
    [ForeignKey("RegistryRefId")]
    public Registry Registry { get; set; } = null!;
    [Column("SourceId")]
    public string CategorySourceId { get; set; } = "";
    public string Description { get; set; } = "";

    public ICollection<Entry> Entries {get; set;} = new List<Entry>();
    public ICollection<Property> Properties {get; set;} = new List<Property>();

}
