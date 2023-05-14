using System.ComponentModel.DataAnnotations;
using ObjModels = CIRLib.ObjectModel.Models;

namespace CIRLIB.UI.Pages
{
    public class EntryViewModel
    {
        public string EntryId { get; set; } = "";    
        public string SourceId { get; set; } = "";
        public string CIRId { get; set; } = "" ;
        public string IdInSource { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool Inactive { get; set; }
        public string CategoryRefId { get; set; } = "";
        public string RegistryRefId { get; set; } = "";
        public string SourceRefId { get; set; } = "";

    }
}
