using AdapterServer.Converters;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace AdapterServer.Extensions;

public static class TypeDescriptorExtensions
{
    public static TypeConverter SelectConverter([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type component, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type toType)
    {
        TypeConverter? converter = null;

        // First, try default method
        if (component.GetCustomAttributes(typeof(TypeConverterAttribute), true).Any())
        {
            converter = TypeDescriptor.GetConverter(component);
        }

        // If there is no `TypeConverter` attribute on the component, then go for Plan B - look for 
        // TypeConverterSelectors and pick the appropriate TypeConverter

        // NB - we're assuming that the requested combination exists, so throw exceptions if things
        // go wrong
        if (converter is null)
        {
            var attributes = component
                .GetCustomAttributes(typeof(TypeConverterSelectorAttribute), true)
                .OfType<TypeConverterSelectorAttribute>()
                .ToList();

            if (!attributes.Any())
                throw new InvalidOperationException($"Component {component.FullName} is missing either TypeConverter or TypeConverterSelector attribute(s)");

            var converterTypeSelectors = attributes.Where(x => x.ToTypeName == toType.AssemblyQualifiedName);

            if (!converterTypeSelectors.Any())
                throw new InvalidOperationException($"No TypeConverter that converts {component.FullName} to {toType.FullName}. Make sure you add the appropriate TypeConverter or TypeConverterSelector attribute(s)");

            var converterTypeSelector = converterTypeSelectors.First();

            var converterType = Type.GetType(converterTypeSelector.ConverterTypeName);
            if (converterType is null)
            {
                throw new InvalidOperationException($"Either Type {converterTypeSelector.ConverterTypeName} does not exist or the appropriate assembly has not been loaded");
            }

            converter = (TypeConverter?)Activator.CreateInstance(converterType);

            if (converter is null)
            {
                throw new InvalidOperationException($"Cannot instantiate {converterTypeSelector.ConverterTypeName} as a TypeConverter");
            }
        }

        if (!converter.CanConvertTo(toType))
        {
            throw new InvalidOperationException($"Cannot use {converter.GetType().FullName} to convert {component.AssemblyQualifiedName} to ${toType.AssemblyQualifiedName}");
        }

        return converter;
    }
}
