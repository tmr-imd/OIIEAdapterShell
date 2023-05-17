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
        public string CategoryRefId { get; set; } = "";
        public string RegistryRefId { get; set; } = "";        
        public string EntryRefIdInSource { get; set; } = "";

        public PropertyViewModel(){
            PropertyId ="";
            PropertyValue ="";
            DataType ="";
        }

    }
}
