using System.ComponentModel;
using Transformation.Test.Converters;

namespace Transformation.Test.Data;

[TypeConverter(typeof(PlumberConverter))]
public record class MarioBrother(string Name, string Description, string SpecialAbility);