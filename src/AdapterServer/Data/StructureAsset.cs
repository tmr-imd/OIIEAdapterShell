using AdapterServer.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdapterServer.Data;

[TypeConverterSelector(typeof(StructureAssetConverter), typeof(Ccom.Asset))]
[TypeConverterSelector(typeof(JsonConverter), typeof(JsonDocument))]
public record class StructureAsset(string Code, string Type, string Location, string Owner, string Condition, string Inspector)
{
    // To support serialization
    public StructureAsset() : this("", "", "", "", "", "") { }
}
