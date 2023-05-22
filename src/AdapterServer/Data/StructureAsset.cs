using AdapterServer.Converters;
using Transformation;

namespace AdapterServer.Data;

[TypeConverterSelector(typeof(StructureAssetConverter), typeof(Ccom.Asset))]
public record class StructureAsset(string Code, string Type, string Location, string Owner, string Condition, string Inspector)
{
    // To support serialization
    public StructureAsset() : this("", "", "", "", "", "") { }
}
