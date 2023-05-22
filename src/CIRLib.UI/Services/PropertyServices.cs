using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using CIRLIB.UI.Pages;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRLib.UI.Services{
public class PropertyServices{
    
    public List<ObjModels.Property> GetAllProperties(CIRLibContext DbContext)
    {     
        return DbContext.Property.OrderByDescending(x => x.DateCreated).ToList();
    }
    public ObjModels.Property GetPropertyById(Guid Id, CIRLibContext DbContext)
    {     
        return DbContext.Property.Where(item => item.Id.Equals(Id)).First(); 
    }
    public void CreateNewProperty( PropertyViewModel newProperty, CIRLibContext DbContext ){
        
        CommonServices.CheckIfRegistryExists(newProperty.RegistryRefId, DbContext);
        CommonServices.CheckIfCategoryExists(newProperty.CategoryRefId, DbContext);
        CommonServices.CheckIfEntryExists(newProperty.EntryRefIdInSource, DbContext);

        var PropertyObj = new ObjModels.Property
        {
            PropertyId = newProperty.PropertyId,
            PropertyValue = newProperty.PropertyValue,
            DataType = newProperty.DataType,
            CategoryRefId = newProperty.CategoryRefId,
            RegistryRefId = newProperty.RegistryRefId,
            EntryRefIdInSource = newProperty.EntryRefIdInSource
        };
        DbContext.Property.Add(PropertyObj);
        DbContext.SaveChanges();
        
    }
    public void UpdateProperty(Guid Id, PropertyViewModel updateProperty, CIRLibContext DbContext ){
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