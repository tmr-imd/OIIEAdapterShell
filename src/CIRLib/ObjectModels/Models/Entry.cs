using System.ComponentModel.DataAnnotations.Schema;
namespace CIRLib.ObjectModel.Models;
public record class Entry : ModelObject
{
    public string IdInSource { get; set; } = "";
    public string SourceId { get; set; } = "";
    public string CIRId { get; set; } = "";
    public string SourceOwnerId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Inactive { get; set; }

    public string CategoryRefId { get; set; } = "";
    [ForeignKey("CategoryRefId")]
    public Category Category {get; set; } = null!;
    public string RegistryRefId { get; set; } = "";
    [ForeignKey("RegistryRefId")]
    public Registry Registry {get; set; } = null!;
    public ICollection<Property> Property {get; set;}

}
