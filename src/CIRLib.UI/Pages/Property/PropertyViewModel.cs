using System.ComponentModel.DataAnnotations;
using ObjModels = CIRLib.ObjectModel.Models;

namespace CIRLIB.UI.Pages
{
    public class PropertyViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Property Id")]
        public string PropertyId { get; set; } = "";

        [Required(AllowEmptyStrings = false)]
        public string PropertyValue { get; set; } = "";

        [Required(AllowEmptyStrings = false)]
        public string DataType { get; set; } = "";
        public string CategoryId { get; set; } = "";
        public string RegistryId { get; set; } = "";        
        public string EntryId { get; set; } = "";

        public static implicit operator ObjModels.Property(PropertyViewModel viewModel)
        {
            return new ObjModels.Property
            {
                PropertyId = viewModel.PropertyId,
                DataType = viewModel.DataType,
                EntryIdInSource = viewModel.EntryId
            };
        }

    }
}
