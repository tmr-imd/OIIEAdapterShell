using System.Text.Json;

namespace Settings.Test;

public class ConfirmationSettingsTest
{
    [Fact]
    public void SerializeDeserializeTest()
    {
        ConfirmationSettings confirmations = new ConfirmationSettings(new ConfirmBODSetting[]
        {
            new ConfirmBODSetting( "*", "*", ConfirmationOptions.Never ),
            new ConfirmBODSetting( "*", "A Topic", ConfirmationOptions.OnError ),
            new ConfirmBODSetting( "/a /channel", "A Topic", ConfirmationOptions.Always )
        });

        var json = JsonSerializer.Serialize(confirmations);

        var readBack = JsonSerializer.Deserialize<ConfirmationSettings>(json);

        Assert.NotNull(readBack);
        Assert.Equal(confirmations.Settings, readBack.Settings);
        // The auto-generated equality for record class does not properly handle IEnumerable
        // Need to decide whether this is an issue or not.
        Assert.Equivalent(confirmations, readBack);
    }

    [Fact]
    public void OrderTest()
    {
        var expected = new ConfirmBODSetting[]
        {
            new ConfirmBODSetting( "*", "*", ConfirmationOptions.Never ),
            new ConfirmBODSetting( "*", "A Topic", ConfirmationOptions.OnError ),
            new ConfirmBODSetting( "/different/channel", "A Topic", ConfirmationOptions.Always ),
            new ConfirmBODSetting( "/some/channel", "*", ConfirmationOptions.OnError ),
            new ConfirmBODSetting( "/some/channel", "A Topic", ConfirmationOptions.Always ),
            new ConfirmBODSetting( "/some/channel", "Different Topic", ConfirmationOptions.Never )
        };

        // Resorts the settings using the ID to get a mixed order.
        var unsorted = expected.OrderBy(s => s.GetId());
        Assert.NotEqual(expected, unsorted);

        var sorted = unsorted.OrderBy(s => s);
        Assert.Equal(expected, sorted);
    }

    [Theory]
    [InlineData(new object[] { "*", "*", ConfirmationOptions.Never })]
    [InlineData(new object[] { "*", "A Topic", ConfirmationOptions.OnError })]
    [InlineData(new object[] { "/different/channel", "A Topic", ConfirmationOptions.Always })]
    [InlineData(new object[] { "/some/channel", "*", ConfirmationOptions.OnError })]
    [InlineData(new object[] { "/some/channel", "A Topic", ConfirmationOptions.Always })]
    [InlineData(new object[] { "/some/channel", "Different Topic", ConfirmationOptions.Never })]
    [InlineData(new object[] { "/some/channel", "Random Topic", ConfirmationOptions.OnError })]
    [InlineData(new object[] { "/different/channel", "Different Topic", ConfirmationOptions.Never })]
    [InlineData(new object[] { "/random/channel", "A Topic", ConfirmationOptions.OnError })]
    public void ConfirmationRuleTest(string channelUri, string topic, ConfirmationOptions expected)
    {
        // Start the rules off unsorted to ensure the check method is working correctly.
        var rules = new ConfirmationSettings(new ConfirmBODSetting[]
        {
            new ConfirmBODSetting( "*", "*", ConfirmationOptions.Never ),
            new ConfirmBODSetting( "*", "A Topic", ConfirmationOptions.OnError ),
            new ConfirmBODSetting( "/different/channel", "A Topic", ConfirmationOptions.Always ),
            new ConfirmBODSetting( "/some/channel", "*", ConfirmationOptions.OnError ),
            new ConfirmBODSetting( "/some/channel", "A Topic", ConfirmationOptions.Always ),
            new ConfirmBODSetting( "/some/channel", "Different Topic", ConfirmationOptions.Never )
        }.OrderBy(s => s.GetId()));

        var result = rules.ConfirmationOptionFor(channelUri, topic);

        Assert.Equal(expected, result);
    }
}