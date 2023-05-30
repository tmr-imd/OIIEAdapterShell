using CommonBOD;
using Oagis;
using System.Xml.Linq;
using CIRNamespace = CIR.Serialization;
using CIR.Serialization;
using CodeType = Ccom.CodeType;
using TextType = Ccom.TextType;
using PropertyObj = CIR.Serialization.Property;
using Ccom;

namespace CIRLib.Test.Fixture;

public class BODTestExamples : IDisposable
{
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
                    new EntryIdentifier 
                    { 
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

    public ShowEquivalentEntriesBOD ShowEquivalentEntries(string bodid, string senderId, DateTime creationTime, string? nounName = null)
    {
      return new ShowEquivalentEntriesBOD()
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
        DataArea = new ShowEquivalentEntriesDataArea
        {
          Show = new ShowType(){},
          GetEquivalentEntriesResponse = new [] {
            new Registry{            
              ID = new IDType() { Value = "Global Corporate Registry" },
              Description = new[] { new TextType() { Value = "Common Registry" }},
              Category = new[] { new Category() {
                ID = new IDType() {Value = "Work Center" },                     
                CategorySourceID = new IDType() {Value = "Work A100" },
                Description = new[] { new TextType() { Value = "Work Center Details"}},
                Entry = new [] { new Entry { 
                  IDInSource = new IDType()  { Value = "Global Corporate Registry" },
                  SourceID =  new IDType() { Value = "Work Center" },
                  CIRID = "Work A100" ,
                  SourceOwnerID = new IDType() { Value = "Transport Company" }, 
                  Name = new TextType() { Value= "Network_N201"},
                  Description = new [] { new TextType(){Value="Network N201 Details"}},
                  Inactive = true,
                  InactiveSpecified = true,
                  Property = new [] { new PropertyObj {
                    ID = new IDType()  { Value = "Server Config" },
                    PropertyValue = new [] { new PropertyValue {
                      Key = new IDType() { Value = "Server Config Values" },
                      Value = "List of config details",
                      UnitOfMeasure = new CodeType() { Value ="CSV_String"}
                    },
                    },                          
                    DataType = new CodeType() { Value = "List"}                 
                  },
                  },
                  
                }
                }
              }
              }
            }
          }
        }
      };
    }


    public string ShowEquivalentEntriesBOD(string bodid, string senderId, DateTime creationTime)
    {
        return $@"<ShowEquivalentEntries xmlns:oa=""http://www.openapplications.org/oagis/9"" xmlns:cir=""http://www.openoandm.org/ws-cir/"" releaseID=""9.0"" languageCode=""en-AU"" xmlns=""http://www.openoandm.org/ws-cir/bod/"">
  <oa:ApplicationArea>
    <oa:Sender>
      <oa:LogicalID>{senderId}</oa:LogicalID>
    </oa:Sender>
    <oa:CreationDateTime>{creationTime.ToXsDateTimeString()}</oa:CreationDateTime>
    <oa:BODID>{bodid}</oa:BODID>
  </oa:ApplicationArea>
  <DataArea>
    <oa:Show />
    <cir:GetEquivalentEntriesResponse>
      <cir:Registry>
        <cir:ID>Global Corporate Registry</cir:ID>
        <cir:Description>Common Registry</cir:Description>
        <cir:Category>
            <cir:ID>Work Center</cir:ID>
            <cir:CategorySourceID>Work A100</cir:CategorySourceID>
            <cir:Description>Work Center Details</cir:Description>
            <cir:Entry>
                <cir:IDInSource>Global Corporate Registry</cir:IDInSource>
                <cir:SourceID>Work Center</cir:SourceID>
                <cir:CIRID>Work A100</cir:CIRID>
                <cir:SourceOwnerID>Transport Company</cir:SourceOwnerID>
                <cir:Name>Network_N201</cir:Name>
                <cir:Description>Network N201 Details</cir:Description>
                <cir:Inactive>true</cir:Inactive>
                <cir:Property>
                    <cir:ID>Server Config</cir:ID>
                    <cir:PropertyValue>
                        <cir:Key>Server Config Values</cir:Key>  
                        <cir:Value>List of config details</cir:Value>
                        <cir:UnitOfMeasure>CSV_String</cir:UnitOfMeasure>
                    </cir:PropertyValue>
                    <cir:DataType>List</cir:DataType>
                </cir:Property>                
            </cir:Entry>              
        </cir:Category>            
      </cir:Registry>
    </cir:GetEquivalentEntriesResponse>
  </DataArea>
</ShowEquivalentEntries>";
    }

}