using AdapterServer.Data;
using System.ComponentModel;
using System.Globalization;

namespace AdapterServer.Converters
{
    public class StructureAssetConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            if (destinationType == typeof(Ccom.Asset))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            var structure = value as StructureAsset;

            if ( destinationType == typeof(Ccom.Asset) && structure is not null )
            {
                return ConvertToAsset( structure );
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private Ccom.Asset ConvertToAsset( StructureAsset structure )
        {
            return new Ccom.Asset()
            {
                ShortName = new[] { 
                    new Ccom.TextType() { 
                        Value = structure.Code 
                }}
            };
        }
    }
}
