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
    public Guid CategoryRefId {get; set;}
    [ForeignKey(nameof(CategoryRefId))]
    public Category Category {get; set; } = null!;
    public Guid RegistryRefId {get; set;}
    [ForeignKey(nameof(RegistryRefId))]
    public Registry Registry {get; set;} = null!;

    // Decided to keep direct FK's to below tables
    // due to functional reasons where comparison with Id would be sufficient
    // and object loading can be avoided.
    public string CategoryId { get; set; } = "";
    public string RegistryId { get; set; } = "";
    public string ParentEntityId { get; set; } = "";
    public string ChildEntityId { get; set; } = "";
    
    public ICollection<Property> Property {get; set;} = new List<Property>();

}
