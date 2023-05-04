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
    [Key, Column(Order = 1)]
    public string CategoryRefId { get; set; } = "";

    [ForeignKey("Registry")]
    [Key, Column(Order = 2)]
    public string RegistryRefId { get; set; } = "";

    
    [ForeignKey("Category")]
    [Key, Column(Order = 3)]
    public string SourceRefId { get; set; } = "";

    [ForeignKey("Entry")]
    [Key, Column(Order = 4)]
    public string SourceId { get; set; } = "";

    [ForeignKey("Entry")]
    [Key, Column(Order = 5)]
     public string IdInSource { get; set; } = "";

}
