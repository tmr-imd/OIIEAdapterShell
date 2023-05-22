using System.ComponentModel;
using System.Globalization;
using Transformation.Test.Data;

namespace Transformation.Test.Converters;

public class PlumberConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(Bio) || sourceType == typeof(MarioBrother))
            return true;

        return base.CanConvertFrom(context, sourceType);
    }
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        if (destinationType == typeof(Plumber))
            return true;

        return base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        return value switch
        {
            Bio bio when destinationType == typeof(Plumber) =>
                new Plumber(bio.Name, bio.Description),

            MarioBrother brother when destinationType == typeof(Plumber) =>
                new Plumber(brother.Name, brother.Description),

            _ =>
                base.ConvertTo(context, culture, value, destinationType)
        };
    }
}
