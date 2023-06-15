using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Oagis;
using CIR.Serialization;

namespace CIR.Serialization;

public partial class ProcessRegistryBOD : BusinessObjectDocumentType {

    [JsonIgnore]
    [XmlIgnore]
    public override XmlSerializerNamespaces Namespaces
    {
        get => new XmlSerializerNamespaces(new[] {
            new XmlQualifiedName("", Namespace),
            new XmlQualifiedName("oa", Oagis.Namespace.URI),
            new XmlQualifiedName("cir", CIR.Serialization.Namespace.URI)
        });
    }

}