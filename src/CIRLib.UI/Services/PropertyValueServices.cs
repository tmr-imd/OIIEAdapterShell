using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using CIRLIB.UI.Pages;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRLib.UI{
public class PropertyValueServices{
    
    public List<ObjModels.PropertyValue> GetAllPropertyValues(CIRLibContext DbContext)
    {     
        return DbContext.PropertyValue.OrderByDescending(x => x.DateCreated).ToList();
    }
    public ObjModels.PropertyValue GetPropertyValueById(string Id, CIRLibContext DbContext)
    {     
        return DbContext.PropertyValue.Where(item => item.Id.Equals(Id)).First(); 
    }
    public void CreateNewPropertyValue( PropertyValueViewModel newProperty, CIRLibContext DbContext ){
        //To do: add FK references
        var PropertyValueObj = new ObjModels.PropertyValue { Key = newProperty.Key,
                                                             Value = newProperty.Value,
                                                             UnitOfMeasure = newProperty.UnitOfMeasure,
                                                             CreatedBy = "authUser",
                                                             DateCreated = DateTime.UtcNow,
                                                             ModifiedBy = "authUser",
                                                             DateModified = DateTime.UtcNow 
                                                            };
        DbContext.PropertyValue.Add(PropertyValueObj);
        DbContext.SaveChanges();
    }
    public void UpdatePropertyValue(string Id, PropertyValueViewModel updateProperty, CIRLibContext DbContext ){
        //To do: add FK references
        var PropertyValueObj = DbContext.PropertyValue.Where(item => item.Id.Equals(Id)).First();
        PropertyValueObj.Key = updateProperty.Key;
        PropertyValueObj.Value = updateProperty.Value;
        PropertyValueObj.UnitOfMeasure = updateProperty.UnitOfMeasure;
        PropertyValueObj.CreatedBy = "authUser";
        PropertyValueObj.DateCreated = DateTime.UtcNow;
        PropertyValueObj.ModifiedBy = "authUser";
        PropertyValueObj.DateModified = DateTime.UtcNow;
        DbContext.SaveChanges();
    }
    public void DeletePropertyValueById(string Id, CIRLibContext DbContext)
    {    
       var DelPropertyValueObj = DbContext.PropertyValue.Where(item => item.Id.Equals(Id)).First(); 
       DbContext.PropertyValue.Remove(DelPropertyValueObj);
       DbContext.SaveChanges();
    }
}
}