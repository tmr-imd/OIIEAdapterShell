namespace CIRLib.ObjectModel.Models;

public record class Registry : ModelObject
{
    public string RegistryId { get; set; } = "";
    public string Description { get; set; } = "";
    public ICollection<Category> Categories {get; set;} = new List<Category>();
    public ICollection<Entry> Entries {get; set;} = new List<Entry>();
    public ICollection<Property> Properties {get; set;} = new List<Property>();

}
