using System.ComponentModel.DataAnnotations;
using ObjModels = CIRLib.ObjectModel.Models;

namespace DataModelServices;

/**
    These models are used to populate data for Input to the Interface.
**/

public class RegistryDef
{
    [Required]
    public string RegistryId { get; set; } = "";
    public string Description { get; set; } = "";

    public static implicit operator ObjModels.Registry(RegistryDef viewModel)
        {
            return new ObjModels.Registry 
            { 
                RegistryId = viewModel.RegistryId,
                Description = viewModel.Description,
                Id = Guid.NewGuid()
            };
        }
}

public class CategoryDef
{
    [Required]
    public string CategoryId { get; set; } = "";
    public string SourceId { get; set; } = "";
    public string Description { get; set; } = "";
    [Required]
    public string RegistryRefId {get; set; }
    public static implicit operator ObjModels.Category(CategoryDef viewModel)
        {
            return new ObjModels.Category
            {
                CategoryId = viewModel.CategoryId,
                CategorySourceId = viewModel.SourceId,
                RegistryRefId = viewModel.RegistryRefId,
                Description = viewModel.Description,
                Id = Guid.NewGuid()
            };
        }
}

public class EntryDef
{
    [Required]
    public string IdInSource { get; set; } = "";
    [Required]
    public string SourceId { get; set; } = "";
    public string CIRId { get; set; } = "";
    public string SourceOwnerId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Inactive { get; set; }
    [Required]
    public string CategoryRefId { get; set; }
    [Required]
    public string RegistryRefId {get; set; }
    public static implicit operator ObjModels.Entry(EntryDef viewModel)
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
            CategoryRefId = viewModel.CategoryRefId,
            RegistryRefId = viewModel.RegistryRefId,
            Id = Guid.NewGuid()
        };
    }
}