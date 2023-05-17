using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using CIRLIB.UI.Pages;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRLib.UI.Services{
public class RegistryServices{
    
    public List<ObjModels.Registry> GetAllRegistries(CIRLibContext DbContext)
    {     
        return DbContext.Registry.OrderByDescending(x => x.DateCreated).ToList();
    }
    public ObjModels.Registry GetRegistryById(Guid Id, CIRLibContext DbContext)
    {     
        return DbContext.Registry.Where(item => item.Id.Equals(Id)).First(); 
    }
    public void CreateNewRegistry( RegistryViewModel newRegistry, CIRLibContext DbContext ){
        var RegistryObj = new ObjModels.Registry { RegistryId = newRegistry.RegistryId,
                                                   Description = newRegistry.Description
                                                    };
        DbContext.Registry.Add(RegistryObj);
        DbContext.SaveChanges();
    }
    public void UpdateRegistry(Guid Id, RegistryViewModel updateRegistry, CIRLibContext DbContext ){
        var RegObj = DbContext.Registry.Where(item => item.Id.Equals(Id)).First();
        RegObj.Description = updateRegistry.Description;
        DbContext.SaveChanges();
    }
    public void DeleteRegistryById(Guid Id, CIRLibContext DbContext)
    {    
       var DelRegObj = DbContext.Registry.Where(item => item.Id.Equals(Id)).First(); 
       DbContext.Registry.Remove(DelRegObj);
       DbContext.SaveChanges();
    }
}
}