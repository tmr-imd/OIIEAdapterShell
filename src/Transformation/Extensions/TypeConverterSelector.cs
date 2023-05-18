using System.ComponentModel;

namespace Transformation.Extensions;

public static class TypeConverterSelector
{
    public static TypeConverter SelectConverter<T>(T component, Type toType)
    {
        TypeConverter? converter = null;

        var fromType = typeof(T);

        var selectors = fromType
            .GetCustomAttributes(typeof(TypeConverterSelectorAttribute), true)
            .OfType<TypeConverterSelectorAttribute>()
            .Where(x => x.ToTypeName == toType.AssemblyQualifiedName);

        converter = selectors
            .Select(x => x.CompatibleConverter(component, toType))
            .Where(x => x is not null)
            .LastOrDefault();

        // Fallback to the default GetConverter call
        converter ??= TypeDescriptor.GetConverter(fromType);

        if (!converter.CanConvertFrom(fromType) || !converter.CanConvertTo(toType))
        {
            throw new InvalidOperationException($"Cannot use {converter.GetType().FullName} to convert {fromType.FullName} to ${toType.FullName}");
        }

        return converter;
    }

    private static TypeConverter? CompatibleConverter<T>(this TypeConverterSelectorAttribute selector, T component, Type toType)
    {
        var converterType = Type.GetType(selector.ConverterTypeName);

        if (converterType is null)
        {
            throw new InvalidOperationException($"Either Type {selector.ConverterTypeName} does not exist or the appropriate assembly has not been loaded");
        }

        var converter = (TypeConverter?)Activator.CreateInstance(converterType);

        if (converter != null)
        {
            var canConvert = converter.CanConvertFrom(typeof(T)) && converter.CanConvertTo(toType);
            var methodPasses = converter.CheckMethodPasses(component, selector.SelectionMethod, selector.SelectionParameter);

            if (canConvert && methodPasses)
                return converter;
        }

        return null;
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
            .GetMethodsBySig(typeof(bool), new[] { typeof(T) })
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
            .GetMethodsBySig(typeof(bool), new[] { typeof(T), typeof(string) })
            .Where(x => x.Name == selectionMethod)
            .FirstOrDefault();

        if (method is null)
            return false;

        var success = (bool?)method.Invoke(converter, new[] { component as object, selectionParameter });

        return success == true;
    }
}
