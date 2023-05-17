using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using CIRLIB.UI.Pages;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRLib.UI.Services{
public class PropertyValueServices{
    
    public List<ObjModels.PropertyValue> GetAllPropertyValues(CIRLibContext DbContext)
    {     
        return DbContext.PropertyValue.OrderByDescending(x => x.DateCreated).ToList();
    }
    public ObjModels.PropertyValue GetPropertyValueById(Guid Id, CIRLibContext DbContext)
    {     
        return DbContext.PropertyValue.Where(item => item.Id.Equals(Id)).First(); 
    }
    public void CreateNewPropertyValue( PropertyValueViewModel newProperty, CIRLibContext DbContext ){
        CommonServices.CheckIfPropertyExists(newProperty.PropertyRefId, DbContext);
        var PropertyValueObj = new ObjModels.PropertyValue {
                                                            Key = newProperty.Key,
                                                            Value = newProperty.Value,
                                                            UnitOfMeasure = newProperty.UnitOfMeasure,
                                                            PropertyRefId = newProperty.PropertyRefId
                                                            };
        DbContext.PropertyValue.Add(PropertyValueObj);
        DbContext.SaveChanges();
    }
    public void UpdatePropertyValue(Guid Id, PropertyValueViewModel updateProperty, CIRLibContext DbContext ){        
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