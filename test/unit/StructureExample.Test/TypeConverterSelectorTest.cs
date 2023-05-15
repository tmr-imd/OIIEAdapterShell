using StructureExample.Test.Converters;
using StructureExample.Test.Data;
using Transformation;

namespace StructureExample.Test;

public class TypeConverterSelectorTest
{
    [Fact]
    public void NoConverter()
    {
        // You can't convert from a Plumber to a MarioBrother. Sad, but true
        var thereIsNoPlumberConverter = () => TypeDescriptorExtensions.SelectConverter( typeof(Plumber), typeof(MarioBrother) );

        Assert.Throws<InvalidOperationException>( thereIsNoPlumberConverter );
    }

    [Fact]
    public void CompatibleWithTypeConverterAttribute()
    {
        var converter = TypeDescriptorExtensions.SelectConverter(typeof(MarioBrother), typeof(Plumber));
        Assert.NotNull(converter);
        Assert.IsType<PlumberConverter>(converter);
    }

    [Fact]
    public void SelectConverterThatDoesNotExist()
    {
        // I.e. there are converters, but none for PiranhaPlant
        var convertersExistButNotToPiranhaPlant = () => TypeDescriptorExtensions.SelectConverter(typeof(Bio), typeof(PiranhaPlant));
        Assert.Throws<InvalidOperationException>(convertersExistButNotToPiranhaPlant);
    }

    [Fact]
    public void SelectConverterThatDoesExist()
    {
        var converter = TypeDescriptorExtensions.SelectConverter( typeof(Bio), typeof(MarioBrother) );
        Assert.NotNull(converter);
        Assert.IsType<SuperMarioConverter>( converter );
    }

    [Fact]
    public void FallbackToTypeConverterAttributeIfSelectionDoesNotExist()
    {
        var converter = TypeDescriptorExtensions.SelectConverter(typeof(Bio), typeof(Plumber));
        Assert.NotNull(converter);
        Assert.IsType<PlumberConverter>(converter);
    }

    [Theory]
    [InlineData(new object[] { "Mario", "", true, true, "Jumping" })]
    [InlineData(new object[] { "Luigi", "", true, true, "Older than Mario" })]
    public void BioToPlumber( string name, string description, bool IsPlumber, bool IsCharacter, string specialAbility )
    {
        var bio = new Bio( name, description, IsPlumber, IsCharacter, specialAbility );

        var converter = TypeDescriptorExtensions.SelectConverter(typeof(Bio), typeof(Plumber));
        var plumber = converter.ConvertTo(bio, typeof(Plumber));

        Assert.NotNull(plumber);
        Assert.IsType<Plumber>(plumber);
    }

    [Theory]
    [InlineData(new object[] { "Mario", "", true, true, "Jumping" })]
    [InlineData(new object[] { "Luigi", "", true, true, "Older than Mario" })]
    public void BioToSuperMario(string name, string description, bool IsPlumber, bool IsCharacter, string specialAbility)
    {
        var bio = new Bio(name, description, IsPlumber, IsCharacter, specialAbility);

        var converter = TypeDescriptorExtensions.SelectConverter(typeof(Bio), typeof(MarioBrother));
        var brother = converter.ConvertTo(bio, typeof(MarioBrother));

        Assert.NotNull(brother);
        Assert.IsType<MarioBrother>(brother);
    }
}