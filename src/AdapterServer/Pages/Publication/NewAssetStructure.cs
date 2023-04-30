using TaskQueueing.Data;

using CommonBOD;
using Oagis;
using Oagis.CodeLists;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace AdapterServer.Pages.Publication;

public record class NewStructureAsset(string Verb, StructureAsset Data)
{
    public XDocument ToSyncStructureAssetsBOD(string? bodid = null, string? senderId = null, DateTime? creationTime = null)
    {
        var bod = new GenericBodType<SyncType, List<StructureAssets>>("SyncStructureAssets", Ccom.Namespace.URI)
        {
            languageCode = "en-AU",
            releaseID = "9.0",
            systemEnvironmentCode = SystemEnvironmentCodeEnumerationType.Production.ToString(),
            ApplicationArea = new ApplicationAreaType()
            {
                BODID = new IdentifierType { Value = bodid ?? Guid.NewGuid().ToString() },
                CreationDateTime = (creationTime?.ToUniversalTime() ?? DateTime.UtcNow).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"),
                Sender = new SenderType
                {
                    LogicalID = new IdentifierType
                    {
                        Value = senderId ?? Guid.NewGuid().ToString()
                    },
                    ConfirmationCode = new ConfirmationResponseCodeType
                    {
                        Value = ConfirmationResponseCodeType.ResponseCodeEnum.OnError.ToString()
                    }
                }
            },
            DataArea = new GenericDataAreaType<SyncType, List<StructureAssets>>()
            {
                Verb = new SyncType(),
                Noun = new List<StructureAssets>
                {
                    new StructureAssets
                    {
                        StructureAsset = new StructureAsset[]
                        {
                            Data
                        }
                    },
                    new StructureAssets
                    {
                        StructureAsset = new StructureAsset[]
                        {
                            Data with { Code = "S2", Type = "Slipway" }
                        }
                    }
                },
            }
        };

        var serializer = bod.CreateSerializer();
        var stream = new StringWriter();
        var settings = new XmlWriterSettings
        {
            ConformanceLevel = ConformanceLevel.Document,
            Encoding = System.Text.Encoding.UTF8,
            Indent = true,
            NewLineOnAttributes = true,
            OmitXmlDeclaration = true
        };
        var doc = new XDocument();
        using (var writer = doc.CreateWriter())
        {
            serializer.Serialize(writer, bod, bod.Namespaces);
        }

        return doc;
    }
}

[XmlType(Namespace = Ccom.Namespace.URI)]
public class StructureAssets
{
    [XmlElement("StructureAsset")]
    public StructureAsset[] StructureAsset { get; set; } = null!;
}
