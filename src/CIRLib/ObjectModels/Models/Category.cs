using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CIRLib.ObjectModel.Models;

public record class Category : ModelObject
{
    [Column(Order = 1), Key]
    public string CategoryId { get; set; } = "";

    [ForeignKey("Registry")]
    [Column(Order = 2), Key]
    public string RegistryRefId { get; set; } = "";
    
    [Column(Order = 3), Key]
    public string SourceId { get; set; } = "";
    public string Description { get; set; } = "";

}
