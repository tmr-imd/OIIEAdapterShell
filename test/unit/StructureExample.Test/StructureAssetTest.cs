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