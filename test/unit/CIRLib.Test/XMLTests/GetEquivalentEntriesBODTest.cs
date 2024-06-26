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
using DataModel = DataModelServices;
namespace CIRLib.Test.XMLTests;

public class GetEquivalentEntriesBODTest : IClassFixture<BODTestSamples>
{
    BODTestSamples examples;
    const string BASE_SCHEMA_PATH = "./XSD";

    readonly BODReaderSettings settings = new BODReaderSettings()
    {
        SchemaPath = BASE_SCHEMA_PATH
    };

    public GetEquivalentEntriesBODTest(BODTestSamples fixture)
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
        Assert.Equal("Enterprise", bod?.DataArea.GetEquivalentEntries.EntryIdentifier.First().RegistryID.Value);
    }

}