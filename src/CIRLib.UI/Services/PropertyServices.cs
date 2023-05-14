using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using CIRLIB.UI.Pages;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRLib.UI{
public class PropertyServices{
    
    public List<ObjModels.Property> GetAllProperties(CIRLibContext DbContext)
    {     
        return DbContext.Property.OrderByDescending(x => x.DateCreated).ToList();
    }
    public ObjModels.Property GetPropertyById(string Id, CIRLibContext DbContext)
    {     
        return DbContext.Property.Where(item => item.Id.Equals(Id)).First(); 
    }
    public void CreateNewProperty( PropertyViewModel newProperty, CIRLibContext DbContext ){
        //To do: add FK references
        var PropertyObj = new ObjModels.Property { PropertyId = newProperty.PropertyId,
                                                   PropertyValue = newProperty.PropertyValue,
                                                   DataType = newProperty.DataType,
                                                   CreatedBy = "authUser",
                                                   DateCreated = DateTime.UtcNow,
                                                   ModifiedBy = "authUser",
                                                   DateModified = DateTime.UtcNow };
        DbContext.Property.Add(PropertyObj);
        DbContext.SaveChanges();
    }
    public void UpdateProperty(string Id, PropertyViewModel updateProperty, CIRLibContext DbContext ){
        //To do: add FK references
        var PropertyObj = DbContext.Property.Where(item => item.Id.Equals(Id)).First();
        PropertyObj.PropertyId = updateProperty.PropertyId;
        PropertyObj.PropertyValue = updateProperty.PropertyValue;
        PropertyObj.DataType = updateProperty.DataType;
        PropertyObj.CreatedBy = "authUser";
        PropertyObj.DateCreated = DateTime.UtcNow;
        PropertyObj.ModifiedBy = "authUser";
        PropertyObj.DateModified = DateTime.UtcNow;
        DbContext.SaveChanges();
    }
    public void DeletePropertyById(string Id, CIRLibContext DbContext)
    {    
       var DelPropertyObj = DbContext.Property.Where(item => item.Id.Equals(Id)).First(); 
       DbContext.Property.Remove(DelPropertyObj);
       DbContext.SaveChanges();
    }
}
}