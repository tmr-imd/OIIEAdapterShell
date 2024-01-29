using System.ComponentModel;

namespace Transformation.Extensions;

public static class TypeConverterSelector
{
    /// <summary>
    /// Selects a TypeConverter that can convert the component object into an
    /// object of the given type. Raises an exception if none can be found.
    /// </summary>
    /// <remarks>
    /// The returned converter my ConvertFrom the type of the component object
    /// to the given type, or it may ConvertTo the given type: i.e., converter
    /// attributes on both types are checked for a match.
    /// </remarks>
    /// <remarks>
    /// If a TypeConverterSelectorAttribute is used and there is a condition
    /// associated, the component object will be tested against the condition
    /// to ensure that it can be converted.
    /// </remarks>
    /// <remarks>
    /// The TypeConverterSelectorAttribute is checked first, if none are found
    /// then the standard type converter attribute is checked using
    /// TypeDescriptor.GetConverter as normal.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static TypeConverter SelectConverter<T>(T component, Type toType)
    {
        var componentType = typeof(T);

        var selectors = SelectorsForType(componentType, toType)
                        .Concat(SelectorsForType(toType, componentType));

        var converter = selectors
            .Select(x => x.Selector.CompatibleConverter(component, toType, x.OwningType == componentType))
            .FirstOrDefault(x => x is not null);

        // Fallback to the default GetConverter call
        converter ??= TypeDescriptor.GetConverter(componentType) is TypeConverter toConverter && toConverter.CanConvertTo(toType) ? toConverter : null;
        converter ??= TypeDescriptor.GetConverter(toType) is TypeConverter fromConverter && fromConverter.CanConvertFrom(componentType) ? fromConverter : null;

        return converter ?? throw new InvalidOperationException($"No suitable type converter class to convert {componentType.FullName} to {toType.FullName}");
    }

    private static IEnumerable<(Type OwningType, TypeConverterSelectorAttribute Selector)> SelectorsForType(Type owningType, Type toType)
    {
        return owningType
            .GetCustomAttributes(typeof(TypeConverterSelectorAttribute), true)
            .OfType<TypeConverterSelectorAttribute>()
            .Where(x => x.ToTypeName == toType.AssemblyQualifiedName)
            .Select(x => (owningType, x));
    } 

    private static TypeConverter? CompatibleConverter<T>(this TypeConverterSelectorAttribute selector, T component, Type toType, bool componentTypeIsAttributeOwner = true)
    {
        var converterType = Type.GetType(selector.ConverterTypeName);

        if (converterType is null)
        {
            throw new InvalidOperationException($"Either Type {selector.ConverterTypeName} does not exist or the appropriate assembly has not been loaded");
        }

        if (Activator.CreateInstance(converterType) is TypeConverter converter
            && converter.CanConvert(typeof(T), toType, componentTypeIsAttributeOwner)
            && converter.CheckMethodPasses(component, selector.SelectionMethod, selector.SelectionParameter))
        {
            return converter;
        }

        return null;
    }

    private static bool CanConvert(this TypeConverter converter, Type componentType, Type toType, bool componentTypeIsAttributeOwner)
    {
        return (componentTypeIsAttributeOwner && converter.CanConvertTo(toType))
                || (!componentTypeIsAttributeOwner && converter.CanConvertFrom(componentType));
    }

    public static bool CheckMethodPasses<T>(this TypeConverter converter, T component, string selectionMethod, string selectionParameter)
    {
        if (component is null || string.IsNullOrEmpty(selectionMethod))
            return true;

        return selectionParameter switch
        {
            var x when !string.IsNullOrEmpty(x) =>
                converter.InvokeCheckMethod(component, selectionMethod, selectionParameter),

            _ =>
                converter.InvokeCheckMethod(component, selectionMethod)
        };
    }
    private static bool InvokeCheckMethod<T>(this TypeConverter converter, T component, string selectionMethod)
    {
        var method = converter.GetType()
            .GetMethodsBySig(typeof(bool), typeof(T))
            .Where(x => x.Name == selectionMethod)
            .FirstOrDefault();

        if (method == null)
            return false;

        var success = (bool?)method.Invoke(converter, new[] { component as object });

        return success == true;
    }
    private static bool InvokeCheckMethod<T>(this TypeConverter converter, T component, string selectionMethod, string selectionParameter)
    {
        var method = converter.GetType()
            .GetMethodsBySig(typeof(bool), typeof(T), typeof(string))
            .Where(x => x.Name == selectionMethod)
            .FirstOrDefault();

        if (method is null)
            return false;

        var success = (bool?)method.Invoke(converter, new[] { component as object, selectionParameter });

        return success == true;
    }
}
