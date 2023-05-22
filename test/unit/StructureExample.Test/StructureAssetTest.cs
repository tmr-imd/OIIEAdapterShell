using AdapterServer.Data;
using CommonBOD;

namespace StructureExample.Test;

public class StructureAssetTest
{
    [Fact]
    public void SyncStructureAssetsBODWriteTest()
    {
        var bodId = Guid.NewGuid().ToString();
        var senderId = Guid.NewGuid().ToString();
        var creationTime = DateTime.UtcNow;
        var expected = SyncStructureAssetsExample(bodId, senderId, creationTime);
        var newStructure = new NewStructureAsset("Sync", new StructureAsset("B1", "Bridge", "Street", "AI", "Good", "Matt"));

        var bod = newStructure.ToSyncStructureAssetsBOD(bodId, senderId, creationTime);
        Assert.Equal(expected, bod.ToString());
    }

    [Fact]
    public void SyncStructureAssetsBODReadTest()
    {
        var bodId = Guid.NewGuid().ToString();
        var senderId = Guid.NewGuid().ToString();
        var creationTime = DateTime.UtcNow;
        var newStructure = new NewStructureAsset("Sync", new StructureAsset("B1", "Bridge", "Street", "AI", "Good", "Matt"));

        var xml = SyncStructureAssetsExample(bodId, senderId, creationTime);

        var bod = new GenericBodType<Oagis.SyncType, List<StructureAssets>>("SyncStructureAssets", Ccom.Namespace.URI);
        var serializer = bod.CreateSerializer();
        using (var reader = new StringReader(xml))
        {
            bod = serializer.Deserialize(reader) as GenericBodType<Oagis.SyncType, List<StructureAssets>>;
        }

        Assert.Equal(bodId, bod?.ApplicationArea.BODID.Value);
        Assert.Equal(newStructure.Data, bod?.DataArea.Noun.First().StructureAsset.First());
    }

    [Fact]
    public void ShowStructureAssetsBODWriteTest()
    {
        var bodId = Guid.NewGuid().ToString();
        var senderId = Guid.NewGuid().ToString();
        var creationTime = DateTime.UtcNow;
        var expected = ShowStructureAssetsExample(bodId, senderId, creationTime);
        var requestStructures = new RequestStructures(new[] { 
            new StructureAsset("B1", "Bridge", "Street", "AI", "Good", "Matt"),
            new StructureAsset("S2", "Slipway", "Street", "AI", "Good", "Matt"),
        }, 2);

        var bod = requestStructures.ToShowStructureAssetsBOD(bodId, senderId, creationTime);
        Assert.Equal(expected, bod.ToString());
    }

    [Fact]
    public void ShowStructureAssetsBODReadTest()
    {
        var bodId = Guid.NewGuid().ToString();
        var senderId = Guid.NewGuid().ToString();
        var creationTime = DateTime.UtcNow;
        var requestStructures = new RequestStructures(new[] { 
            new StructureAsset("B1", "Bridge", "Street", "AI", "Good", "Matt"),
            new StructureAsset("S2", "Slipway", "Street", "AI", "Good", "Matt"),
        }, 2);

        var xml = ShowStructureAssetsExample(bodId, senderId, creationTime);

        var bod = new GenericBodType<Oagis.ShowType, List<RequestStructures>>("ShowStructureAssets", Ccom.Namespace.URI);
        var serializer = bod.CreateSerializer();
        using (var reader = new StringReader(xml))
        {
            bod = serializer.Deserialize(reader) as GenericBodType<Oagis.ShowType, List<RequestStructures>>;
        }

        Assert.Equal(bodId, bod?.ApplicationArea.BODID.Value);
        Assert.Equal(requestStructures.Count, bod?.DataArea.Noun.First().Count);
        Assert.Equal(requestStructures.StructureAssets, bod?.DataArea.Noun.First().StructureAssets ?? Enumerable.Empty<StructureAsset>());
        Assert.Equal(requestStructures.StructureAssetsForXml, bod?.DataArea.Noun.First().StructureAssetsForXml);
        // Auto-generated Equals checks IEnumerable as references only?
        // Assert.Equal(requestStructures, bod?.DataArea.Noun.First());
    }

    [Fact]
    public void GetStructureAssetsBODWriteTest()
    {
        var bodId = Guid.NewGuid().ToString();
        var senderId = Guid.NewGuid().ToString();
        var creationTime = DateTime.UtcNow;
        var expected = GetStructureAssetsExample(bodId, senderId, creationTime);
        var filter = new StructureAssetsFilter("B1", "Bridge", "Street", "AI", "Good", "Matt");

        var bod = filter.ToGetStructureAssetsBOD(bodId, senderId, creationTime);
        Assert.Equal(expected, bod.ToString());
    }


    [Fact]
    public void GetStructureAssetsBODReadTest()
    {
        var bodId = Guid.NewGuid().ToString();
        var senderId = Guid.NewGuid().ToString();
        var creationTime = DateTime.UtcNow;
        var filter = new StructureAssetsFilter("B1", "Bridge", "Street", "AI", "Good", "Matt");

        var xml = GetStructureAssetsExample(bodId, senderId, creationTime);

        var bod = new GenericBodType<Oagis.GetType, List<StructureAssetsFilter>>("GetStructureAssets", Ccom.Namespace.URI, nounName: "StructureAssetsFilter");
        var serializer = bod.CreateSerializer();
        using (var reader = new StringReader(xml))
        {
            bod = serializer.Deserialize(reader) as GenericBodType<Oagis.GetType, List<StructureAssetsFilter>>;
        }

        Assert.Equal(bodId, bod?.ApplicationArea.BODID.Value);
        Assert.Equal(filter, bod?.DataArea.Noun.First());
    }

    public string GetStructureAssetsExample(string bodid, string senderId, DateTime creationTime)
    {
        return $@"<GetStructureAssets xmlns:oa=""http://www.openapplications.org/oagis/9"" releaseID=""9.0"" languageCode=""en-AU"" xmlns=""http://www.mimosa.org/ccom4"">
  <oa:ApplicationArea>
    <oa:Sender>
      <oa:LogicalID>{senderId}</oa:LogicalID>
      <oa:ConfirmationCode>OnError</oa:ConfirmationCode>
    </oa:Sender>
    <oa:CreationDateTime>{creationTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'")}</oa:CreationDateTime>
    <oa:BODID>{bodid}</oa:BODID>
  </oa:ApplicationArea>
  <DataArea>
    <oa:Get>
      <oa:Expression>//*</oa:Expression>
    </oa:Get>
    <StructureAssetsFilter>
      <FilterCode>B1</FilterCode>
      <FilterType>Bridge</FilterType>
      <FilterLocation>Street</FilterLocation>
      <FilterOwner>AI</FilterOwner>
      <FilterCondition>Good</FilterCondition>
      <FilterInspector>Matt</FilterInspector>
    </StructureAssetsFilter>
  </DataArea>
</GetStructureAssets>";
    }

    public string ShowStructureAssetsExample(string bodid, string senderId, DateTime creationTime)
    {
        return $@"<ShowStructureAssets xmlns:oa=""http://www.openapplications.org/oagis/9"" releaseID=""9.0"" languageCode=""en-AU"" xmlns=""http://www.mimosa.org/ccom4"">
  <oa:ApplicationArea>
    <oa:Sender>
      <oa:LogicalID>{senderId}</oa:LogicalID>
      <oa:ConfirmationCode>Never</oa:ConfirmationCode>
    </oa:Sender>
    <oa:CreationDateTime>{creationTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'")}</oa:CreationDateTime>
    <oa:BODID>{bodid}</oa:BODID>
  </oa:ApplicationArea>
  <DataArea>
    <oa:Show />
    <StructureAssets>
      <Count>2</Count>
      <StructureAsset>
        <Code>B1</Code>
        <Type>Bridge</Type>
        <Location>Street</Location>
        <Owner>AI</Owner>
        <Condition>Good</Condition>
        <Inspector>Matt</Inspector>
      </StructureAsset>
      <StructureAsset>
        <Code>S2</Code>
        <Type>Slipway</Type>
        <Location>Street</Location>
        <Owner>AI</Owner>
        <Condition>Good</Condition>
        <Inspector>Matt</Inspector>
      </StructureAsset>
    </StructureAssets>
  </DataArea>
</ShowStructureAssets>";
    }

    public string SyncStructureAssetsExample(string bodid, string senderId, DateTime creationTime)
    {
        return $@"<SyncStructureAssets xmlns:oa=""http://www.openapplications.org/oagis/9"" releaseID=""9.0"" languageCode=""en-AU"" xmlns=""http://www.mimosa.org/ccom4"">
  <oa:ApplicationArea>
    <oa:Sender>
      <oa:LogicalID>{senderId}</oa:LogicalID>
      <oa:ConfirmationCode>OnError</oa:ConfirmationCode>
    </oa:Sender>
    <oa:CreationDateTime>{creationTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'")}</oa:CreationDateTime>
    <oa:BODID>{bodid}</oa:BODID>
  </oa:ApplicationArea>
  <DataArea>
    <oa:Sync />
    <StructureAssets>
      <StructureAsset>
        <Code>B1</Code>
        <Type>Bridge</Type>
        <Location>Street</Location>
        <Owner>AI</Owner>
        <Condition>Good</Condition>
        <Inspector>Matt</Inspector>
      </StructureAsset>
    </StructureAssets>
    <StructureAssets>
      <StructureAsset>
        <Code>S2</Code>
        <Type>Slipway</Type>
        <Location>Street</Location>
        <Owner>AI</Owner>
        <Condition>Good</Condition>
        <Inspector>Matt</Inspector>
      </StructureAsset>
    </StructureAssets>
  </DataArea>
</SyncStructureAssets>";
    }
}