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
                CreationDateTime = (creationTime?.ToUniversalTime() ?? DateTime.UtcNow).ToXsDateTimeString(),
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
                Verb = new GetType
                {
                    Expression = new[]
                    {
                        new ExpressionType { Value = "//*" }
                    }
                },
                Noun = new List<StructureAssetsFilter>()
                {
                    self
                }
            }
        };

        return bod.SerializeToDocument();
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
                CreationDateTime = (creationTime?.ToUniversalTime() ?? DateTime.UtcNow).ToXsDateTimeString(),
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

        return bod.SerializeToDocument();
    }
}