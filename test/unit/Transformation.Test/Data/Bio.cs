using System.ComponentModel;
using Transformation.Test.Converters;

namespace Transformation.Test.Data;

[TypeConverter(typeof(PlumberConverter))]
[TypeConverterSelector(typeof(SuperMarioConverter), typeof(MarioBrother), SelectionMethod = "IsCharacter")]
[TypeConverterSelector(typeof(SuperMarioConverter), typeof(MarioBrother), SelectionMethod = "IsPlumber", SelectionParameter = "true")]
public record class Bio(string Name, string Description, bool IsPlumber, bool IsCharacter, string SpecialAbility);
