using AdapterServer.Converters;
using StructureExample.Test.Converters;
using System.ComponentModel;

namespace StructureExample.Test.Data;

[TypeConverter(typeof(PlumberConverter))]
[TypeConverterSelector(typeof(SuperMarioConverter), typeof(MarioBrother))]
public record class Bio( string Name, string Description, bool IsPlumber, bool IsCharacter, string SpecialAbility );
