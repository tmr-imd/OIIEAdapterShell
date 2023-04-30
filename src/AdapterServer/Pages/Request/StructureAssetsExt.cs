using CommonBOD;
using Oagis;
using System.Xml.Linq;

namespace TaskQueueing.Data;

public static class StructureAssetsExt
{
    public static XDocument ToGetStructureAssetsBOD(this StructureAssetsFilter self, string? bodid = null, string? senderId = null, DateTime? creationTime = null)
    {
        var bod = new GenericBodType<GetType, List<StructureAssetsFilter>>("GetStructureAssets", Ccom.Namespace.URI, nounName: "StructureAssetsFilter")
        {
            languageCode = "en-AU",
            releaseID = "9.0",
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
            DataArea = new GenericDataAreaType<GetType, List<StructureAssetsFilter>>()
            {
                Verb = new GetType(),
                Noun = new List<StructureAssetsFilter>()
                {
                    self
                }
            }
        };

        return bod.serialize();
    }

    public static XDocument ToShowStructureAssetsBOD(this RequestStructures self, string? bodid = null, string? senderId = null, DateTime? creationTime = null)
    {
        var bod = new GenericBodType<ShowType, List<RequestStructures>>("ShowStructureAssets", Ccom.Namespace.URI)
        {
            languageCode = "en-AU",
            releaseID = "9.0",
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
                        Value = ConfirmationResponseCodeType.ResponseCodeEnum.Never.ToString()
                    }
                }
            },
            DataArea = new GenericDataAreaType<ShowType, List<RequestStructures>>()
            {
                Verb = new ShowType(),
                Noun = new List<RequestStructures>()
                {
                    self
                }
            }
        };

        return bod.serialize();
    }

    private static XDocument serialize<TVerb, TNoun>(this GenericBodType<TVerb, TNoun> bod)
        where TVerb : VerbType, new()
        where TNoun : class, new()
    {
        var doc = new XDocument();
        using (var writer = doc.CreateWriter())
        {
            bod.CreateSerializer().Serialize(writer, bod, bod.Namespaces);
        }
        return doc;
    }
}