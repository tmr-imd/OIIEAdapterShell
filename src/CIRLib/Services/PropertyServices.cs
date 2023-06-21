using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;

namespace CIRServices;
  
public class PropertyServices : CommonServices
{
    public List<ObjModels.Property> GetAllProperties(CIRLibContext DbContext)
    {     
        return DbContext.Property.OrderByDescending(x => x.DateCreated).ToList();
    }
    public ObjModels.Property GetPropertyById(Guid Id, CIRLibContext DbContext)
    {     
        return DbContext.Property.Where(item => item.Id.Equals(Id)).First(); 
    }
    public List<ObjModels.Property> GetPropertiesFromFilters(string registryId, string categoryId, string entryId,
        string propertyId, string propertyValueKey, CIRLibContext dbContext)
    {     
       IQueryable<ObjModels.Property> Query = dbContext.Property;

        if(!string.IsNullOrWhiteSpace(registryId))
        {
            Query = Query.Where(
                    Result => Result.Entry.Category.Registry.RegistryId == registryId
                );
        }

        if(!string.IsNullOrWhiteSpace(categoryId))
        {
            Query = Query.Where(
                    Result => Result.Entry.Category.CategoryId == categoryId
                );
        }

        if(!string.IsNullOrWhiteSpace(entryId))
        {
            Query = Query.Where(
                    Result => Result.EntryIdInSource == entryId
                );
        }

        if(!string.IsNullOrWhiteSpace(propertyValueKey))
        {
            Query = Query.Join(
                    dbContext.PropertyValue, 
                    p => p.Id,
                    pv => pv.PropertyRefId,
                    (p,pv) => new 
                    {
                        Property = p,
                        PropertyValue = pv
                    }
                ).Where(
                    joinResult => joinResult.PropertyValue.Key == propertyValueKey
                ).Select(
                    joinResult => joinResult.Property
                );
        }

        if(!string.IsNullOrWhiteSpace(propertyId))
        {
            Query = Query.Where(
                Result => Result.PropertyId == propertyId
            );
        }
        return Query.ToList();
    }

    public void CreateNewProperty(ObjModels.Property newProperty, CIRLibContext dbContext)
    {            
        var entryObjExists = CheckIfEntryExists(newProperty.EntryIdInSource, dbContext, "create");
        if(entryObjExists == null)
        {
            //We expect a valid EntryIdInSource to be provided.
            //Otherwise, we error out.

            // We can create an Entry on the fly but the CategoryRefId and RegistryRefId is not available.
            throw new ArgumentException("EntryIdInSource is not Valid.");
        }
        else
        {
            newProperty.Entry = entryObjExists;
        }
        
        newProperty.Id = Guid.NewGuid();
        dbContext.Property.Add(newProperty);
        dbContext.SaveChanges();
    }
    public void UpdateProperty(Guid Id, ObjModels.Property updateProperty, CIRLibContext dbContext)
    {
        _ = CheckIfRegistryExists(updateProperty.Entry.Category.Registry.RegistryId, dbContext, "update");
        _ = CheckIfCategoryExists(updateProperty.Entry.Category.CategoryId, dbContext, "update");
        var entryExists = CheckIfEntryExists(updateProperty.EntryIdInSource, dbContext, "update");

        var PropertyObj = dbContext.Property.Where(item => item.Id.Equals(Id)).First();
        PropertyObj.PropertyValue = updateProperty.PropertyValue;
        PropertyObj.DataType = updateProperty.DataType;
        dbContext.SaveChanges();
    }
    public void DeletePropertyById(Guid Id, CIRLibContext DbContext)
    {
        var DelPropertyObj = DbContext.Property.Where(item => item.Id.Equals(Id)).First();
        DbContext.Property.Remove(DelPropertyObj);
        DbContext.SaveChanges();
    }
}