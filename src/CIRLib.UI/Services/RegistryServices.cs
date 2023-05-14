using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using CIRLIB.UI.Pages;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRLib.UI{
public class RegistryServices{
    
    public List<ObjModels.Registry> GetAllRegistries(CIRLibContext DbContext)
    {     
        return DbContext.Registry.OrderByDescending(x => x.DateCreated).ToList();
    }
    public ObjModels.Registry GetRegistryById(string Id, CIRLibContext DbContext)
    {     
        return DbContext.Registry.Where(item => item.Id.Equals(Id)).First(); 
    }
    public void CreateNewRegistry( RegistryViewModel newRegistry, CIRLibContext DbContext ){
        var RegistryObj = new ObjModels.Registry { RegistryId = newRegistry.RegistryId,
                                                   Description = newRegistry.Description,
                                                   CreatedBy = "authUser",
                                                   DateCreated = DateTime.UtcNow,
                                                   ModifiedBy = "authUser",
                                                   DateModified = DateTime.UtcNow };
        DbContext.Registry.Add(RegistryObj);
        DbContext.SaveChanges();
    }
    public void UpdateRegistry(string Id, RegistryViewModel updateRegistry, CIRLibContext DbContext ){
        var RegObj = DbContext.Registry.Where(item => item.Id.Equals(Id)).First();
        RegObj.Description = updateRegistry.Description;
        RegObj.ModifiedBy = "authUser";
        RegObj.DateModified = DateTime.UtcNow;
        DbContext.SaveChanges();
    }
    public void DeleteRegistryById(string Id, CIRLibContext DbContext)
    {    
       var DelRegObj = DbContext.Registry.Where(item => item.Id.Equals(Id)).First(); 
       DbContext.Registry.Remove(DelRegObj);
       DbContext.SaveChanges();
    }
}
}