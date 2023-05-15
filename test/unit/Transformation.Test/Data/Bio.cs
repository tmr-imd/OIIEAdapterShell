using System.ComponentModel;
using Transformation;
using Transformation.Test.Converters;

namespace Transformation.Test.Data;

[TypeConverter(typeof(PlumberConverter))]
[TypeConverterSelector(typeof(SuperMarioConverter), typeof(MarioBrother))]
public record class Bio(string Name, string Description, bool IsPlumber, bool IsCharacter, string SpecialAbility);
