using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace CIRLib.ObjectModel.Models;
public record class PropertyValue : ModelObject
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string UnitOfMeasure { get; set; } = "";

    public Guid PropertyRefId { get; set; }
    [ForeignKey("PropertyRefId")]
    public Property Property { get; set; } = null!;
    public string PropertyId { get; set; } = "";

    public T? ValueFromJson<T>() => JsonSerializer.Deserialize<T>(Value, JsonSerializerOptions.Default);
    public void ValueFromJson<T>(T newValue) => Value = JsonSerializer.Serialize(newValue, JsonSerializerOptions.Default);
}
