using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CIRLib.ObjectModel.Models;
public record class Entry : ModelObject
{
    public string EntryId { get; set; } = "";
    
    [Key]
    [Column(Order = 4)]
    public string SourceId { get; set; } = "";
    public string CIRId { get; set; } = "";

    [Key]
    [Column(Order = 5)]
     public string IdInSource { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Inactive { get; set; }

    [ForeignKey("Category")]
    [Key, Column(Order = 1)]
    public string CategoryRefId { get; set; } = "";

    [ForeignKey("Registry")]
    [Key, Column(Order = 2)]
    public string RegistryRefId { get; set; } = "";

    
    [ForeignKey("Category")]
    [Key, Column(Order = 3)]
    public string SourceRefId { get; set; } = "";

}
