using CommonBOD;
using Oagis;
using System.Xml.Linq;
using CIRNamespace = CIR.Serialization;
using CIR.Serialization;
using Ccom;

namespace CIRLib.Test.Fixture;

public class BODExamples : IDisposable
{
    const string DATA_PATH = "/Lib/CCOM.Net/data/example_bod_sync_segments.xml";

    public void Dispose()
    {
        // Do nothing
    }

    public (string BodId, string SenderId, DateTime CreationDateTime) GenerateApplicationAreaFields()
    {
        return (Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), DateTime.UtcNow);
    }

    public GetEquivalentEntriesBOD GetEquivalentEntries(string bodid, string senderId, DateTime creationTime, string? nounName = null)
    {
        return new GetEquivalentEntriesBOD()
        {
            releaseID = "9.0",
            languageCode = "en-AU",
            ApplicationArea = new ApplicationAreaType
            {
                BODID = new IdentifierType { Value = bodid },
                CreationDateTime = creationTime.ToXsDateTimeString(),
                Sender = new SenderType
                {
                    LogicalID = new IdentifierType { Value = senderId }
                }
            },
            DataArea = new GetEquivalentEntriesDataArea
            {
                Process = new ProcessType(){},
                GetEquivalentEntries = new GetEquivalentEntries()
                {
                        EntryIdentifier = new[] {
                            new EntryIdentifier { 
                                                  RegistryID = new IDType()  { Value = "Enterprise" },
                                                  CategoryID =  new IDType() { Value = "EnterpriseCategory" },
                                                  CategorySourceID = new IDType() { Value = "EnterpriseCategory10" } ,
                                                  EntryIDInSource =new IDType() { Value = "EntryEnterprise10" }, 
                                                  EntrySourceID =new IDType() { Value = "EntrySource10" }
                                                }
                                              },
                        TargetSourceID = new[] {
                            new IDType() { Value = "ID" }
                        }
                }
            }
        };
    }

    public string GetEquivalentEntriesBOD(string bodid, string senderId, DateTime creationTime)
    {
        return $@"<GetEquivalentEntries xmlns:oa=""http://www.openapplications.org/oagis/9"" xmlns:cir=""http://www.openoandm.org/ws-cir/"" releaseID=""9.0"" languageCode=""en-AU"" xmlns=""http://www.openoandm.org/ws-cir/bod/"">
  <oa:ApplicationArea>
    <oa:Sender>
      <oa:LogicalID>{senderId}</oa:LogicalID>
    </oa:Sender>
    <oa:CreationDateTime>{creationTime.ToXsDateTimeString()}</oa:CreationDateTime>
    <oa:BODID>{bodid}</oa:BODID>
  </oa:ApplicationArea>
  <DataArea>
    <oa:Process />
    <cir:GetEquivalentEntries>
      <cir:EntryIdentifier>
        <cir:RegistryID>Enterprise</cir:RegistryID>
        <cir:CategoryID>EnterpriseCategory</cir:CategoryID>
        <cir:CategorySourceID>EnterpriseCategory10</cir:CategorySourceID>
        <cir:EntryIDInSource>EntryEnterprise10</cir:EntryIDInSource>
        <cir:EntrySourceID>EntrySource10</cir:EntrySourceID>
      </cir:EntryIdentifier>
      <cir:TargetSourceID>ID</cir:TargetSourceID>
    </cir:GetEquivalentEntries>
  </DataArea>
</GetEquivalentEntries>";
    }

    public string SyncSegments()
    {
        return File.ReadAllText($"{DATA_PATH}/example_bod_sync_segments.xml");
    }

}