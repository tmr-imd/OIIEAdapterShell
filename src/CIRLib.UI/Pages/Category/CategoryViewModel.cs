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
        [Display(Name = "RegistryId")]
        public string RegistryId { get; set; } = "";

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "SourceId")]
        public string CategorySourceId { get; set; } = "";
        
        public string Description { get; set; } = "";

        public CategoryViewModel()
        {
            CategoryId = "";
            CategorySourceId = "";
            RegistryId = "";
            Description = "";
        }

        public static implicit operator ObjModels.Category(CategoryViewModel viewModel)
        {
            return new ObjModels.Category
            {
                CategoryId = viewModel.CategoryId,
                CategorySourceId = viewModel.CategorySourceId,
                RegistryId = viewModel.RegistryId,
                Description = viewModel.Description
            };
        }
    }
}
