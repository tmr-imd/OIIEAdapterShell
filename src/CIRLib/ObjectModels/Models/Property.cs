using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIRLib.ObjectModel.Models;
public record class Property : ModelObject
{
    [Key]
    public string PropertyId { get; set; } = "";
    public string PropertyValue { get; set; } = "";
    public string DataType { get; set; } = "";
    
    [ForeignKey("Category")]
    [Column(Order = 1),Key]
    public string CategoryRefId { get; set; } = "";

    [ForeignKey("Registry")]
    [Column(Order = 2), Key]
    public string RegistryRefId { get; set; } = "";

    
    [ForeignKey("Category")]
    [Column(Order = 3), Key]
    public string SourceRefId { get; set; } = "";

    [ForeignKey("Entry")]
    [Column(Order = 4), Key]
    public string SourceId { get; set; } = "";

    [ForeignKey("Entry")]
    [Column(Order = 5), Key]
     public string IdInSource { get; set; } = "";

}
