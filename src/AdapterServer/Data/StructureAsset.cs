using AdapterServer.Converters;
using System.ComponentModel;

namespace AdapterServer.Data;

[TypeConverter(typeof(StructureAssetConverter))]
public record class StructureAsset(string Code, string Type, string Location, string Owner, string Condition, string Inspector)
{
    // To support serialization
    public StructureAsset() : this("", "", "", "", "", "") { }
}
