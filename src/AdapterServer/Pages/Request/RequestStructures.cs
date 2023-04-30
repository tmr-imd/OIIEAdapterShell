using System.Xml.Serialization;

namespace TaskQueueing.Data;

[XmlType(Namespace = Ccom.Namespace.URI)]
public record RequestStructures(IEnumerable<StructureAsset> StructureAssets, int? Count = null)
{
    // To support XML serialization
    [XmlIgnore]
    public IEnumerable<StructureAsset> StructureAssets { get; init; } = StructureAssets;

    [XmlElement("Count", Order = 0)]
    public int? Count { get; init; } = Count;

    public RequestStructures() : this(Enumerable.Empty<StructureAsset>(), 0) { }

    [XmlElement("StructureAsset", Order = 1)]
    public StructureAsset[] StructureAssetsForXml
    {
        get => StructureAssets.ToArray();
        init => StructureAssets = value;
    }
}
