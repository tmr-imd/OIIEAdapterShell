using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRServices{
public class PropertyServices{
    
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
        public void CreateNewProperty(ObjModels.Property newProperty, CIRLibContext DbContext)
        {
            CommonServices.CheckIfRegistryExists(newProperty.RegistryRefId, DbContext);
            CommonServices.CheckIfCategoryExists(newProperty.CategoryRefId, DbContext);
            CommonServices.CheckIfEntryExists(newProperty.EntryRefIdInSource, DbContext);
            DbContext.Property.Add(newProperty);
            DbContext.SaveChanges();
        }
        public void UpdateProperty(Guid Id, ObjModels.Property updateProperty, CIRLibContext DbContext)
        {
            CommonServices.CheckIfRegistryExists(updateProperty.RegistryRefId, DbContext);
            CommonServices.CheckIfCategoryExists(updateProperty.CategoryRefId, DbContext);
            CommonServices.CheckIfEntryExists(updateProperty.EntryRefIdInSource, DbContext);

            var PropertyObj = DbContext.Property.Where(item => item.Id.Equals(Id)).First();
            PropertyObj.PropertyId = updateProperty.PropertyId;
            PropertyObj.PropertyValue = updateProperty.PropertyValue;
            PropertyObj.DataType = updateProperty.DataType;
            PropertyObj.CategoryRefId = updateProperty.CategoryRefId;
            PropertyObj.RegistryRefId = updateProperty.RegistryRefId;
            PropertyObj.EntryRefIdInSource = updateProperty.EntryRefIdInSource;
            DbContext.SaveChanges();
        }
        public void DeletePropertyById(Guid Id, CIRLibContext DbContext)
        {
            var DelPropertyObj = DbContext.Property.Where(item => item.Id.Equals(Id)).First();
            DbContext.Property.Remove(DelPropertyObj);
            DbContext.SaveChanges();
        }
}
}