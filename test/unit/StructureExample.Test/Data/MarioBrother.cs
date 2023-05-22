using StructureExample.Test.Converters;
using System.ComponentModel;

namespace StructureExample.Test.Data;

[TypeConverter(typeof(PlumberConverter))]
public record class MarioBrother(string Name, string Description, string SpecialAbility);