using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CIRLib.ObjectModel.Models;

public record class Category : ModelObject
{
    [Key]
    [Column(Order = 1)]
    public string CategoryId { get; set; } = "";

    [ForeignKey("Registry")]
    [Key]
    [Column(Order = 2)]
    public string RegistryRefId { get; set; } = "";
    
    [Key]
    [Column(Order = 3)]
    public string SourceId { get; set; } = "";
    public string Description { get; set; } = "";

}
