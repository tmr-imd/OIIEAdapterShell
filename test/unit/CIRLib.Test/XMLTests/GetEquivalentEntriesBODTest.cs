using CommonBOD;
using Oagis;
using System.Xml;
using System.Xml.Linq;
using CIRLib.Test.Fixture;
using Ccom;
using CIR.Serialization;
using CIRLib.Persistence;
using System.Security.Claims;
using CIRServices;
using ObjModels = CIRLib.ObjectModel.Models;
using Microsoft.Extensions.Configuration;
using CIRLib.Test;
using CIRLib.Test.Fixture;
namespace CIRLib.Test.XMLTests;

public class GetEquivalentEntriesBODTest : IClassFixture<BODTestExamples>
{
    BODTestExamples examples;
    const string BASE_SCHEMA_PATH = "./XSD";

    readonly BODReaderSettings settings = new BODReaderSettings()
    {
        SchemaPath = BASE_SCHEMA_PATH
    };

    public GetEquivalentEntriesBODTest(BODTestExamples fixture)
    {
        this.examples = fixture;
    }

    [Fact]
    public void SerializeToDocumentTest()
    {
        var (bodId, senderId, creationDateTime) = examples.GenerateApplicationAreaFields();
        var expected = XDocument.Parse(examples.GetEquivalentEntriesBOD(bodId, senderId, creationDateTime));
        var bod = examples.GetEquivalentEntries(bodId, senderId, creationDateTime);
        Assert.Equal(expected, bod.SerializeToDocument(), new XNodeEqualityComparer());
    }

    [Fact]
    public void SerializeToStringTest()
    {
        var (bodId, senderId, creationDateTime) = examples.GenerateApplicationAreaFields();
        var expected = examples.GetEquivalentEntriesBOD(bodId, senderId, creationDateTime);
        var bod = examples.GetEquivalentEntries(bodId, senderId, creationDateTime);
        Assert.Equal(expected, bod.SerializeToString());
    }

    [Fact]
    public void DeserializeTest()
    {
        var (bodId, senderId, creationDateTime) = examples.GenerateApplicationAreaFields();
        var source = new StringReader(examples.GetEquivalentEntriesBOD(bodId, senderId, creationDateTime));
        BODReader reader = new BODReader(source, "", settings);
        Assert.True(reader.IsValid);

        var bod = reader.AsBod<GetEquivalentEntriesBOD>();
        Assert.NotNull(bod);
        Assert.Equal("Enterprise", bod.DataArea.GetEquivalentEntries.EntryIdentifier.First().RegistryID.Value);
    }

    [Fact]
    public void AddAndGetEquivalentEntriesTest()
    {
        //GetEquivalentEntriesTest gets invoked inside AddEntriesTest
        AddEntriesTest();
    }
    
    public void GetEquivalentEntriesTest(CIRLibContext dBContext)
    {   
        var details = new
        {
            IdInSource = "Network1",
            SourceId = "NetworkCat"
        };
        var ListOfEntries = CIRManager.GetEquivalentEntries(details,dBContext);
        
        Assert.Equal("Bridge", ListOfEntries.First().RegistryRefId);
    }

    
    public void AddEntriesTest()
    {   
        var rService = new RegistryServices();
        var cService = new CategoryServices();
        var eService = new EntryServices();
        var dbContext = new MockContextFactory().GetDbContext();

        var details = new
        {
            RegistryId = "Bridge",
            CategoryId = "CatOfBridges",
            RegistryRefId = "Bridge",
            IdInSource = "Network1",
            SourceId = "NetworkCat",
            CategoryRefId = "CatOfBridges"
        };
        CIRManager.AddEntries(details, dbContext);

        var AssertEntryObj = dbContext.Entry.Where(item => item.IdInSource.Equals("Network1")).First();
        Assert.Equal("Network1", AssertEntryObj.IdInSource);
        
        //Testing the below with the above data.
        GetEquivalentEntriesTest(dbContext);
    }

    [Fact]
    public void ModifyEntries()
    {
        bool AddToLocalCacheOnly;
        var updateEntry= new ObjModels.Entry();
        var EService = new EntryServices();
        var DbContext = new CIRLibContextFactory().CreateDbContext(new ClaimsPrincipal()).Result;
        EService.UpdateEntry(updateEntry.Id, updateEntry, DbContext);

        var AssertEntryObj = DbContext.Entry.Where(item => item.Id.Equals(updateEntry.Id)).First();
        Assert.Equal(AssertEntryObj.Id,updateEntry.Id);

    }
}