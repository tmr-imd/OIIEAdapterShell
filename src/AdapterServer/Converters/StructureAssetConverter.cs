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

        private static Ccom.Asset ConvertToAsset( StructureAsset structure )
        {
            return new Ccom.Asset()
            {
                UUID = new Ccom.UUID(autogen: true),
                ShortName = new[] { 
                    new Ccom.TextType() { 
                        Value = structure.Code 
                }},
                Type = new Ccom.AssetType()
                {
                    UUID = new Ccom.UUID(autogen: true),
                    FullName = new[] { new Ccom.TextType() { Value = structure.Type} } 
                },
                RegistrationSite = new Ccom.Site()
                { 
                    UUID = new Ccom.UUID(autogen: true), 
                    FullName = new[] { new Ccom.TextType() { Value = structure.Location } }
                },
                AssetOwnerEvent = new[] {
                    new Ccom.AssetOwnerEvent()
                    {
                        UUID = new Ccom.UUID(autogen: true),
                        FullName = new[] { new Ccom.TextType() { Value = structure.Owner} }
                    }
                },
                HealthAssessment = new[] 
                { 
                    new Ccom.HealthAssessment() 
                    { 
                        UUID = new Ccom.UUID(autogen : true),
                        FullName = new[] { new Ccom.TextType() { Value = structure.Condition } },
                    } 
                },
                //AssetOwnerEvent = new[]
                //{
                //    new Ccom.AssetOwnerEvent()
                //    {
                //        FullName = new[] {
                //            new Ccom.TextType()
                //            {
                //                Value = structure.Owner,
                //            }
                //        }
                //    }
                //}
                //Description = new Ccom.TextType()
                //{
                //    Value = structure.
                //}
            };
        }
    }
}
