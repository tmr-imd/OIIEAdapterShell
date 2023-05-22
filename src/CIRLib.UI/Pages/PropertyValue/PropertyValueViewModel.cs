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
        public string PropertyRefId { get; set; } = "";
    }
}
