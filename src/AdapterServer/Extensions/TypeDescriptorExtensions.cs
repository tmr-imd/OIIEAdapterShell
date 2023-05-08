using AdapterServer.Converters;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace AdapterServer.Extensions;

public static class TypeDescriptorExtensions
{
    public static TypeConverter SelectConverter([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type componentType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type toType)
    {
        TypeConverter? converter = null;

        var attributes = componentType
            .GetCustomAttributes(typeof(TypeConverterSelectorAttribute), true)
            .OfType<TypeConverterSelectorAttribute>()
            .ToList();

        if ( attributes.Any() )
        {
            var converterTypeSelectors = attributes.Where(x => x.ToTypeName == toType.AssemblyQualifiedName);

            if (converterTypeSelectors.Any())
            {
                var converterTypeSelector = converterTypeSelectors.First();

                var converterType = Type.GetType(converterTypeSelector.ConverterTypeName);
                if (converterType is null)
                {
                    throw new InvalidOperationException($"Either Type {converterTypeSelector.ConverterTypeName} does not exist or the appropriate assembly has not been loaded");
                }

                converter = (TypeConverter?)Activator.CreateInstance(converterType);
            }
        }

        if (converter is null)
        {
            converter = TypeDescriptor.GetConverter(componentType);
        }

        if (!converter.CanConvertFrom(componentType) || !converter.CanConvertTo(toType))
        {
            throw new InvalidOperationException($"Cannot use {converter.GetType().FullName} to convert {componentType.FullName} to ${toType.FullName}");
        }

        return converter;
    }
}
