using CommonBOD;
using Oagis;
using System.Xml;
using System.Xml.Linq;
using CIRLib.Test.Fixture;
using CIR.Serialization;
using Ccom;

namespace CIRLib.Test.XMLTests;

public class ShowEquivalentEntriesBODTest : IClassFixture<BODTestSamples>
{
    BODTestSamples examples;
    const string BASE_SCHEMA_PATH = "./XSD";

    readonly BODReaderSettings settings = new BODReaderSettings()
    {
        SchemaPath = BASE_SCHEMA_PATH
    };

    public ShowEquivalentEntriesBODTest(BODTestSamples fixture)
    {
        this.examples = fixture;
    }

    [Fact]
    public void SerializeToDocumentTest()
    {
        var (bodId, senderId, creationDateTime) = examples.GenerateApplicationAreaFields();
        var expected = XDocument.Parse(examples.ShowEquivalentEntriesBOD(bodId, senderId, creationDateTime));
        var bod = examples.ShowEquivalentEntries(bodId, senderId, creationDateTime);
        Assert.Equal(expected, bod.SerializeToDocument(), new XNodeEqualityComparer());
    }

    [Fact]
    public void SerializeToStringTest()
    {
        var (bodId, senderId, creationDateTime) = examples.GenerateApplicationAreaFields();
        var expected = examples.ShowEquivalentEntriesBOD(bodId, senderId, creationDateTime);
        var bod = examples.ShowEquivalentEntries(bodId, senderId, creationDateTime);
        Assert.Equal(expected, bod.SerializeToString());
    }

    [Fact]
    public void DeserializeTest()
    {
        var (bodId, senderId, creationDateTime) = examples.GenerateApplicationAreaFields();
        var expected = XDocument.Parse(examples.ShowEquivalentEntriesBOD(bodId, senderId, creationDateTime));
        var deserialized = ShowEquivalentEntriesBOD.Deserialize<ShowEquivalentEntriesBOD>(expected);
        Assert.Equal("Global Corporate Registry", deserialized?.DataArea.GetEquivalentEntriesResponse.First().ID.Value);
    }
    
}