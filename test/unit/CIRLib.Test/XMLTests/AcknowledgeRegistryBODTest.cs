using CommonBOD;
using Oagis;
using System.Xml;
using System.Xml.Linq;
using CIRLib.Test.Fixture;
using CIR.Serialization;
using Ccom;

namespace CIRLib.Test.XMLTests;

public class AcknowledgeRegistryBODTest : IClassFixture<BODTestSamples>
{
    BODTestSamples examples;
    const string BASE_SCHEMA_PATH = "./XSD";

    readonly BODReaderSettings settings = new BODReaderSettings()
    {
        SchemaPath = BASE_SCHEMA_PATH
    };

    public AcknowledgeRegistryBODTest(BODTestSamples fixture)
    {
        this.examples = fixture;
    }

    [Fact]
    public void SerializeToDocumentTest()
    {
        var (bodId, senderId, creationDateTime) = examples.GenerateApplicationAreaFields();
        var expected = XDocument.Parse(examples.AcknowledgeRegistryBOD(bodId, senderId, creationDateTime));
        var bod = examples.AcknowledgeRegistry(bodId, senderId, creationDateTime);
        Assert.Equal(expected, bod.SerializeToDocument(), new XNodeEqualityComparer());
    }

    [Fact]
    public void SerializeToStringTest()
    {
        var (bodId, senderId, creationDateTime) = examples.GenerateApplicationAreaFields();
        var expected = examples.AcknowledgeRegistryBOD(bodId, senderId, creationDateTime);
        var bod = examples.AcknowledgeRegistry(bodId, senderId, creationDateTime);
        Assert.Equal(expected, bod.SerializeToString());
    }

    [Fact]
    public void DeserializeTest()
    {
        var (bodId, senderId, creationDateTime) = examples.GenerateApplicationAreaFields();
        var expected = XDocument.Parse(examples.AcknowledgeRegistryBOD(bodId, senderId, creationDateTime));
        var deserialized = AcknowledgeRegistryBOD.Deserialize<AcknowledgeRegistryBOD>(expected);

        Assert.Equal("Internal Error while creation.", deserialized.DataArea.CreateRegistryFault.First().Description.Value);
        Assert.Equal("Internal Error while creation.", deserialized.DataArea.CreateCategoryFault.First().Description.Value);
        Assert.Equal("Entry already exists.", deserialized.DataArea.DuplicateEntryFault.First().Description.Value);
        Assert.Equal("Property already exists.", deserialized.DataArea.DuplicatePropertyFault.First().Description.Value);
    }
}