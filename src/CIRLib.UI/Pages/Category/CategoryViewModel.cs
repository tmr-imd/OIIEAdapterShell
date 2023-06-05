using System.ComponentModel.DataAnnotations;
using ObjModels = CIRLib.ObjectModel.Models;

namespace CIRLIB.UI.Pages
{
    public class CategoryViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "CategoryId")]
        public string CategoryId { get; set; } = "";
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "RegistryRefId")]
        public string RegistryRefId { get; set; } = "";

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "SourceId")]
        public string CategorySourceId { get; set; } = "";
        
        public string Description { get; set; } = "";

        public CategoryViewModel()
        {
            CategoryId = "";
            CategorySourceId = "";
            RegistryRefId = "";
            Description = "";
        }

        public static implicit operator ObjModels.Category(CategoryViewModel viewModel)
        {
            return new ObjModels.Category
            {
                CategoryId = viewModel.CategoryId,
                CategorySourceId = viewModel.CategorySourceId,
                RegistryRefId = viewModel.RegistryRefId,
                CategoryDescription = viewModel.Description,
                Id = Guid.NewGuid()
            };
        }
    }
}
