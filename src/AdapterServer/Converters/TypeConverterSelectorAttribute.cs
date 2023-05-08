using System.Diagnostics;
using System.Globalization;

namespace AdapterServer.Converters;

/// <devdoc>
///    <para>Specifies what type to use as
///       a converter for the object
///       this
///       attribute is bound to. This class cannot
///       be inherited.</para>
/// </devdoc>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public sealed class TypeConverterSelectorAttribute : Attribute {
    private readonly string converterTypeName;
    private readonly string toTypeName;

    /// <devdoc>
    ///    <para> Specifies the type to use as
    ///       a converter for the object this attribute is bound to. This
    ///    <see langword='static '/>field is read-only. </para>
    /// </devdoc>
    public static readonly TypeConverterSelectorAttribute Default = new();
 
    /// <devdoc>
    ///    <para>
    ///       Initializes a new instance of the <see cref='System.ComponentModel.TypeConverterAttribute'/> class with the
    ///       default type converter, which
    ///       is an
    ///       empty string ("").
    ///    </para>
    /// </devdoc>
    public TypeConverterSelectorAttribute() {
        converterTypeName = string.Empty;
        toTypeName = string.Empty;
    }

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.ComponentModel.TypeConverterAttribute'/> class, using 
    ///    the specified type as the data converter for the object this attribute
    ///    is bound
    ///    to.</para>
    /// </devdoc>
    public TypeConverterSelectorAttribute(Type fromType, Type toType) {
        converterTypeName = fromType.AssemblyQualifiedName ?? "";
        toTypeName = toType.AssemblyQualifiedName ?? "";
    }

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.ComponentModel.TypeConverterAttribute'/> class, using 
    ///    the specified type name as the data converter for the object this attribute is bound to.</para>
    /// </devdoc>
    public TypeConverterSelectorAttribute(string fromTypeName, string toTypeName) {
        string temp = fromTypeName.ToUpper(CultureInfo.InvariantCulture);
        Debug.Assert(temp.Contains(".DLL"), "Came across: " + fromTypeName + " . Please remove the .dll extension");
        this.converterTypeName = fromTypeName;

        temp = toTypeName.ToUpper(CultureInfo.InvariantCulture);
        Debug.Assert(temp.Contains(".DLL"), "Came across: " + toTypeName + " . Please remove the .dll extension");
        this.toTypeName = toTypeName;
    }

    /// <devdoc>
    /// <para>Gets the fully qualified type name of the <see cref='System.Type'/>
    /// to use as a converter for the object this attribute
    /// is bound to.</para>
    /// </devdoc>
    public string ConverterTypeName => converterTypeName;

    /// <devdoc>
    /// <para>Gets the fully qualified type name of the <see cref='System.Type'/>
    /// to use as a converter for the object this attribute
    /// is bound to.</para>
    /// </devdoc>
    public string ToTypeName => toTypeName;

    public override bool Equals(object? obj) {
        return (obj is TypeConverterSelectorAttribute other) && other.ConverterTypeName == converterTypeName && other.ToTypeName == toTypeName;
    }
 
    public override int GetHashCode()
    {
        return HashCode.Combine(converterTypeName, toTypeName);
    }
}
