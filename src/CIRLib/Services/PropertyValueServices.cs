using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRServices{
public class PropertyValueServices : CommonServices
{
    public List<ObjModels.PropertyValue> GetAllPropertyValues(CIRLibContext DbContext)
    {     
        return DbContext.PropertyValue.OrderByDescending(x => x.DateCreated).ToList();
    }
    public ObjModels.PropertyValue GetPropertyValueById(Guid Id, CIRLibContext DbContext)
    {     
        return DbContext.PropertyValue.Where(item => item.Id.Equals(Id)).First(); 
    }

    public List<ObjModels.PropertyValue> GetPropertyValuesFromFilters(string RegistryId, string CategoryId, string EntryId,
        string PropertyId, string PropertyValueKey, CIRLibContext DbContext)
    {     
        IQueryable<ObjModels.PropertyValue> Query = DbContext.PropertyValue;
        
        if( !string.IsNullOrWhiteSpace(RegistryId) || !string.IsNullOrWhiteSpace(CategoryId) ||
            !string.IsNullOrWhiteSpace(EntryId) || !string.IsNullOrWhiteSpace(PropertyId))
        {
            var GroupQuery = Query.Join(
                DbContext.Property, 
                pv => pv.PropertyRefId,
                p => p.Id,
                (pv,p) => new 
                {
                    PropertyValue = pv,
                    Property = p
                }
                );
        
            if(!string.IsNullOrWhiteSpace(RegistryId))
            {
                GroupQuery = GroupQuery.Where
                (
                    joinResult => joinResult.Property.Entry.RegistryId == RegistryId
                );
            }

            if(!string.IsNullOrWhiteSpace(CategoryId))
            {
                GroupQuery = GroupQuery.Where
                (
                    joinResult => joinResult.Property.Entry.CategoryId == CategoryId
                );
            }

            if(!string.IsNullOrWhiteSpace(EntryId))
            {
                GroupQuery = GroupQuery.Where
                (
                    joinResult => joinResult.Property.EntryIdInSource == EntryId
                );
            }

            if(!string.IsNullOrWhiteSpace(PropertyId))
            {
                GroupQuery = GroupQuery.Where(
                    Result => Result.Property.PropertyId == PropertyId
                    );
            }
            
            Query = GroupQuery.Select(joinResult => joinResult.PropertyValue);
        }

        if(!string.IsNullOrWhiteSpace(PropertyValueKey))
        {
            Query = Query.Where(
                joinResult => joinResult.Key == PropertyValueKey
                );
        }
        return Query.ToList();
    
    }
    public void CreateNewPropertyValue(ObjModels.PropertyValue propertyValueObj, 
        ObjModels.Property propertyObj = null!, CIRLibContext dbContext = null!)
    {
        //PropertyValue is created along with Property.
        //Ideally, we could have clubbed both as a single service
        //but for now, propertyObj will always be passed into the function.
        
        if(propertyObj == null)
        {
            var existingPropertyObj = dbContext.Property.FirstOrDefault(
                item => item.PropertyId.Contains(propertyValueObj.PropertyId));
            if(existingPropertyObj == null )
            {
                // Ideally the code should not reach here.
                // Do not want to create a property on the fly because 
                // then the entry category registry RefIds are needed.
            }
            else
            {
                propertyValueObj.Property = existingPropertyObj;
            }
        }
        else
        {
            propertyValueObj.Property = propertyObj;
        }
        
        propertyValueObj.Id = Guid.NewGuid();
        dbContext.PropertyValue.Add(propertyValueObj);
        dbContext.SaveChanges();
    }

    public void UpdatePropertyValue(Guid Id, ObjModels.PropertyValue updateProperty, CIRLibContext dbContext )
    {   
        _ = CheckIfPropertyExists(updateProperty.PropertyId, dbContext);

        var PropertyValueObj = dbContext.PropertyValue.Where(item => item.Id.Equals(Id)).First();
        PropertyValueObj.Key = updateProperty.Key;
        PropertyValueObj.Value = updateProperty.Value;
        PropertyValueObj.UnitOfMeasure = updateProperty.UnitOfMeasure;
        dbContext.SaveChanges();        
    }

    public void DeletePropertyValueById(Guid Id, CIRLibContext DbContext)
    {    
       var DelPropertyValueObj = DbContext.PropertyValue.Where(item => item.Id.Equals(Id)).First(); 
       DbContext.PropertyValue.Remove(DelPropertyValueObj);
       DbContext.SaveChanges();
    }
}
}