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
        //ForeignKey("Category")
        public string SourceRefId { get; set; } = "";
        //ForeignKey("Entry")
        public string SourceId { get; set; } = "";
        //ForeignKey("Entry")
         public string IdInSource { get; set; } = "";

        public PropertyViewModel(){
            PropertyId ="";
            PropertyValue ="";
            DataType ="";
        }

    }
}
