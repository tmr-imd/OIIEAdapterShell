using System.ComponentModel.DataAnnotations;
using ObjModels = CIRLib.ObjectModel.Models;

namespace CIRLIB.UI.Pages
{
    public class PropertyValueViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Key")]
        public string Key { get; set; } = "";

        [Required(AllowEmptyStrings = false)]
        public string Value { get; set; } = "";

        [Required(AllowEmptyStrings = false)]
        public string UnitOfMeasure { get; set; } = "";
        public string PropertyId { get; set; } = "";

        public static implicit operator ObjModels.PropertyValue(PropertyValueViewModel viewModel)
        {
            return new ObjModels.PropertyValue
            {
                Key = viewModel.Key,
                Value = viewModel.Value,
                UnitOfMeasure = viewModel.UnitOfMeasure,
                PropertyId = viewModel.PropertyId
            };
        }
    }
}
