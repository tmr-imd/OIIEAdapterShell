using Transformation.Extensions;
using Transformation.Test.Converters;
using Transformation.Test.Data;

namespace Transformation.Test;

public class TypeConverterSelectorTest
{
    public static IEnumerable<object[]> GetPlumbers()
    {
        yield return new object[] { "Mario", "The iconic plumber and hero of the Mushroom Kingdom, known for his red hat, mustache, and heroic adventures." };
        yield return new object[] { "Luigi", "Luigi is the timid yet loyal younger brother of Mario, known for his high jumps and green attire." };
    }

    public static IEnumerable<object[]> GetBios()
    {
        yield return new object[] { "Mario", "The iconic plumber and hero of the Mushroom Kingdom, known for his red hat, mustache, and heroic adventures.", true, true, "Triple Jump" };
        yield return new object[] { "Luigi", "Luigi is the timid yet loyal younger brother of Mario, known for his high jumps and green attire.", true, true, "Poltergust" };
    }

    [Theory]
    [MemberData(nameof(GetPlumbers))]
    public void NoConverter(string name, string description)
    {
        var plumber = new Plumber(name, description);

        // You can't convert from a Plumber to a MarioBrother. Sad, but true
        var thereIsNoPlumberConverter = () => TypeConverterSelector.SelectConverter(plumber, typeof(MarioBrother));

        Assert.Throws<InvalidOperationException>(thereIsNoPlumberConverter);
    }

    [Theory]
    [MemberData(nameof(GetBios))]
    public void SelectConverterThatDoesNotExist(string name, string description, bool IsPlumber, bool IsCharacter, string specialAbility)
    {
        var bio = new Bio(name, description, IsPlumber, IsCharacter, specialAbility);

        // I.e. there are converters, but none for PiranhaPlant
        var convertersExistButNotToPiranhaPlant = () => TypeConverterSelector.SelectConverter(bio, typeof(PiranhaPlant));
        Assert.Throws<InvalidOperationException>(convertersExistButNotToPiranhaPlant);
    }

    [Theory]
    [MemberData(nameof(GetBios))]
    public void SelectSuperMarioConverter(string name, string description, bool IsPlumber, bool IsCharacter, string specialAbility)
    {
        var bio = new Bio(name, description, IsPlumber, IsCharacter, specialAbility);

        var converter = TypeConverterSelector.SelectConverter(bio, typeof(MarioBrother));
        Assert.NotNull(converter);
        Assert.IsType<SuperMarioConverter>(converter);
    }

    [Theory]
    [MemberData(nameof(GetBios))]
    public void FallbackToTypeConverterAttributeIfSelectionDoesNotExist(string name, string description, bool IsPlumber, bool IsCharacter, string specialAbility)
    {
        var bio = new Bio(name, description, IsPlumber, IsCharacter, specialAbility);

        var converter = TypeConverterSelector.SelectConverter(bio, typeof(Plumber));
        Assert.NotNull(converter);
        Assert.IsType<PlumberConverter>(converter);
    }
}