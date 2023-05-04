using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CIRLib.ObjectModel.Models;

public record class Registry : ModelObject
{
    [Key]
    [Column(Order = 2)]
    public string RegistryId { get; set; } = "";
    public string Description { get; set; } = "";

}
