using System.ComponentModel.DataAnnotations;
using ObjModels = CIRLib.ObjectModel.Models;

namespace CIRLIB.UI.Pages
{
    public class EntryViewModel
    {
        public string IdInSource { get; set; } = "";    
        public string SourceId { get; set; } = "";
        public string CIRId { get; set; } = "" ;
        public string SourceOwnerId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool Inactive { get; set; }
        public string CategoryRefId { get; set; } = "";
        public string RegistryRefId { get; set; } = "";

        public static implicit operator ObjModels.Entry(EntryViewModel viewModel)
        {
            return new ObjModels.Entry
            {
                IdInSource = viewModel.IdInSource,
                SourceId = viewModel.SourceId,
                CIRId = viewModel.CIRId,
                SourceOwnerId = viewModel.SourceOwnerId,
                Name = viewModel.Name,
                EntryDescription = viewModel.Description,
                Inactive = viewModel.Inactive,
                CategoryRefId = viewModel.CategoryRefId,
                RegistryRefId = viewModel.RegistryRefId,
                Id = Guid.NewGuid()
            };
        }
    }
}
