﻿using System.ComponentModel;
using System.Globalization;
using Transformation.Test.Data;

namespace Transformation.Test.Converters;

public class SuperMarioConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(Bio))
            return true;

        return base.CanConvertFrom(context, sourceType);
    }
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        if (destinationType == typeof(MarioBrother))
            return true;

        return base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        return value switch
        {
            Bio bio when destinationType == typeof(MarioBrother) =>
                new MarioBrother(bio.Name, bio.Description, bio.SpecialAbility),

            _ =>
                base.ConvertTo(context, culture, value, destinationType)
        };
    }

    public bool IsPlumber( Bio instance, string value )
    {
        var boolValue = bool.Parse(value);

        return instance.IsPlumber == boolValue;
    }

    public bool IsPlumber( Bio instance )
    {
        return instance.IsPlumber;
    }
}
