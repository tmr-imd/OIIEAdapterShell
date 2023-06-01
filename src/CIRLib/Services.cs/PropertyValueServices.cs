using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRServices{
public class PropertyValueServices{
    
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
                p => p.PropertyId,
                (pv,p) => new 
                {
                    PropertyValue = pv,
                    Property = p
                }
                );
        
            if(!string.IsNullOrWhiteSpace(RegistryId))
            {
                GroupQuery = GroupQuery.Where(
                    joinResult => joinResult.Property.RegistryRefId == RegistryId
                    );
            }

            if(!string.IsNullOrWhiteSpace(CategoryId))
            {
                GroupQuery = GroupQuery.Where(
                    joinResult => joinResult.Property.CategoryRefId == CategoryId
                    );
            }

            if(!string.IsNullOrWhiteSpace(EntryId))
            {
                GroupQuery = GroupQuery.Where(
                    joinResult => joinResult.Property.EntryRefIdInSource == EntryId
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
    public void CreateNewPropertyValue(ObjModels.PropertyValue PropertyValueObj, CIRLibContext DbContext )
    {
        CommonServices.CheckIfPropertyExists(PropertyValueObj.PropertyRefId, DbContext);
        
        DbContext.PropertyValue.Add(PropertyValueObj);
        DbContext.SaveChanges();
    }
    public void UpdatePropertyValue(Guid Id, ObjModels.PropertyValue updateProperty, CIRLibContext DbContext )
    {        
        CommonServices.CheckIfPropertyExists(updateProperty.PropertyRefId, DbContext);

        var PropertyValueObj = DbContext.PropertyValue.Where(item => item.Id.Equals(Id)).First();
        PropertyValueObj.Key = updateProperty.Key;
        PropertyValueObj.Value = updateProperty.Value;
        PropertyValueObj.UnitOfMeasure = updateProperty.UnitOfMeasure;
        PropertyValueObj.PropertyRefId = updateProperty.PropertyRefId;
        DbContext.SaveChanges();        
    }
    public void DeletePropertyValueById(Guid Id, CIRLibContext DbContext)
    {    
       var DelPropertyValueObj = DbContext.PropertyValue.Where(item => item.Id.Equals(Id)).First(); 
       DbContext.PropertyValue.Remove(DelPropertyValueObj);
       DbContext.SaveChanges();
    }
}
}