using System.ComponentModel.DataAnnotations;
using ObjModels = CIRLib.ObjectModel.Models;

namespace CIRLIB.UI.Pages;

public class RegistryViewModel
{
    [Required(AllowEmptyStrings = false)]
    [Display(Name = "Registry Id")]
    public string RegistryId { get; set; } = "";

    [Required(AllowEmptyStrings = false)]
    public string Description { get; set; } = "";

    public RegistryViewModel(){
        RegistryId ="";
        Description ="";
    }

    public static implicit operator ObjModels.Registry(RegistryViewModel viewModel)
    {
        return new ObjModels.Registry 
        { 
            RegistryId = viewModel.RegistryId,
            Description = viewModel.Description,
            Id = Guid.NewGuid()
        };
    }

}
