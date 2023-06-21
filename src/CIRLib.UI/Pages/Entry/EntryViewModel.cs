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
        public string CategoryId { get; set; } = "";
        public string RegistryId { get; set; } = "";

        public static implicit operator ObjModels.Entry(EntryViewModel viewModel)
        {
            return new ObjModels.Entry
            {
                IdInSource = viewModel.IdInSource,
                SourceId = viewModel.SourceId,
                CIRId = viewModel.CIRId,
                SourceOwnerId = viewModel.SourceOwnerId,
                Name = viewModel.Name,
                Description = viewModel.Description,
                Inactive = viewModel.Inactive,
                CategoryId = viewModel.CategoryId,
                RegistryId = viewModel.RegistryId
            };
        }
    }
}
