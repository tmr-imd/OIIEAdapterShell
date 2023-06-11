using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRServices{
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
    public List<ObjModels.Property> GetPropertiesFromFilters(string RegistryId, string CategoryId, string EntryId,
        string PropertyId, string PropertyValueKey, CIRLibContext DbContext)
    {     
       IQueryable<ObjModels.Property> Query = DbContext.Property;

        if(!string.IsNullOrWhiteSpace(RegistryId))
        {
            Query = Query.Where(
                    Result => Result.RegistryRefId == RegistryId
                );
        }

        if(!string.IsNullOrWhiteSpace(CategoryId))
        {
            Query = Query.Where(
                    Result => Result.CategoryRefId == CategoryId
                );
        }

        if(!string.IsNullOrWhiteSpace(EntryId))
        {
            Query = Query.Where(
                    Result => Result.EntryRefIdInSource == EntryId
                );
        }

        if(!string.IsNullOrWhiteSpace(PropertyValueKey))
        {
            Query = Query.Join(
                    DbContext.PropertyValue, 
                    p => p.PropertyId,
                    pv => pv.PropertyRefId,
                    (p,pv) => new 
                    {
                        Property = p,
                        PropertyValue = pv
                    }
                ).Where(
                    joinResult => joinResult.PropertyValue.Key == PropertyValueKey
                ).Select(
                    joinResult => joinResult.Property
                );
        }

        if(!string.IsNullOrWhiteSpace(PropertyId))
        {
            Query = Query.Where(
                Result => Result.PropertyId == PropertyId
            );
        }
        return Query.ToList();
    }
    public void CreateNewProperty(ObjModels.Property newProperty, CIRLibContext dbContext)
    {            
        var entryExists = CheckIfEntryExists(newProperty.EntryRefIdInSource, dbContext, "create");
        if(entryExists == null)
        {
            var entryObj = new ObjModels.Entry()
            {
                IdInSource = newProperty.EntryRefIdInSource
            };
            dbContext.Entry.Add(entryObj);
        }
        
        newProperty.Id = Guid.NewGuid();
        dbContext.Property.Add(newProperty);
        dbContext.SaveChanges();
    }
    public void UpdateProperty(Guid Id, ObjModels.Property updateProperty, CIRLibContext dbContext)
    {
        _ = CheckIfRegistryExists(updateProperty.RegistryRefId, dbContext, "update");
        _ = CheckIfCategoryExists(updateProperty.CategoryRefId, dbContext, "update");
        var entryExists = CheckIfEntryExists(updateProperty.EntryRefIdInSource, dbContext, "update");

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
}