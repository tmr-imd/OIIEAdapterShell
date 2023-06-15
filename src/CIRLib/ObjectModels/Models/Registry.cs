namespace CIRLib.ObjectModel.Models;

public record class Registry : ModelObject
{
    public string RegistryId { get; set; } = "";
    public string Description { get; set; } = "";
    public ICollection<Category> Categories {get; set;} = null!;
    public ICollection<Entry> Entries {get; set;} = null!;
    public ICollection<Property> Properties {get; set;} = null!;

}
