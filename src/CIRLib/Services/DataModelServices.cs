using System.ComponentModel.DataAnnotations;
using ObjModels = CIRLib.ObjectModel.Models;

namespace DataModelServices;

/**
    These models are used to populate data for Input to the Interface.
**/

public class RegistryDef
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string RegistryId { get; set; } = "";
    public string Description { get; set; } = "";

    public static implicit operator ObjModels.Registry(RegistryDef viewModel)
    {
        return new ObjModels.Registry 
        { 
            RegistryId = viewModel.RegistryId,
            Description = viewModel.Description
        };
    }
}

public class CategoryDef
{
    [Required]
    public string CategoryId { get; set; } = "";
    [Required]
    public string SourceId { get; set; } = "";
    public string Description { get; set; } = "";
    [Required]
    public string RegistryId {get; set; } = null!;
    public static implicit operator ObjModels.Category(CategoryDef viewModel)
    {
        return new ObjModels.Category
        {
            CategoryId = viewModel.CategoryId,
            CategorySourceId = viewModel.SourceId,
            RegistryId = viewModel.RegistryId,
            Description = viewModel.Description
        };
    }

    public static implicit operator CategoryDef(ObjModels.Category category)
    {
        return new CategoryDef
        {
            CategoryId = category.CategoryId,
            Description = category.Description,
            RegistryId = category.RegistryId,
            SourceId = category.CategorySourceId
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
    public string CategoryId { get; set; } = null!;
    [Required]
    public string CategorySourceId { get; set; } = null!;
    [Required]
    public string RegistryId {get; set; } = null!;

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
            CategoryId = viewModel.CategoryId,
            RegistryId = viewModel.RegistryId
        };
    }

    public static implicit operator EntryDef(ObjModels.Entry entry)
    {
        return new EntryDef
        {
            CategoryId = entry.CategoryId,
            CategorySourceId = entry.Category.CategorySourceId,
            CIRId = entry.CIRId,
            Description = entry.Description,
            IdInSource = entry.IdInSource,
            Inactive = entry.Inactive,
            Name = entry.Name,
            RegistryId = entry.RegistryId,
            SourceId = entry.SourceId,
            SourceOwnerId = entry.SourceOwnerId
        };
    }

    public override string ToString()
    {
        return "EntryId: "+ IdInSource+", "+"SourceId: "+ SourceId+", "+"CIRID: "+ CIRId;
    }
}

public class PropertyDef
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string PropertyId { get; set; } = "";
    public string DataType { get; set; } = "";
    [Required]
    public string EntryIdInSource { get; set; } = "";
    public string Key { get; set; } = "";

    [Required]
    public string Value { get; set; } = "";

    public string UnitOfMeasure { get; set; } = "";

    public static implicit operator ObjModels.Property(PropertyDef viewModel)
    {
        var property = new ObjModels.Property
        {
            PropertyId = viewModel.PropertyId,
            EntryIdInSource = viewModel.EntryIdInSource,
            DataType = viewModel.DataType,
        };

        // Add the value if there is one
        if (!string.IsNullOrWhiteSpace(viewModel.Value))
        {
            property.PropertyValues.Add(viewModel);
        }

        return property;
    }

    public static implicit operator ObjModels.PropertyValue(PropertyDef viewModel)
    {
        return new ObjModels.PropertyValue
        {
            PropertyId = viewModel.PropertyId,
            Key = viewModel.Key,
            UnitOfMeasure = viewModel.UnitOfMeasure,
            Value = viewModel.Value,
        };
    }

    public override string ToString()
    {
        return "PropertyId: "+ PropertyId+", "+"EntryRefId: "+ EntryIdInSource;
    }
}
