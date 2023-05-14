using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using CIRLIB.UI.Pages;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRLib.UI{
public class EntryServices{
    
    public List<ObjModels.Entry> GetAllEntries(CIRLibContext DbContext)
    {     
        return DbContext.Entry.OrderByDescending(x => x.DateCreated).ToList();
    }
    public ObjModels.Entry GetEntryById(string Id, CIRLibContext DbContext)
    {     
        return DbContext.Entry.Where(item => item.Id.Equals(Id)).First(); 
    }
    public void CreateNewEntry( EntryViewModel newEntry, CIRLibContext DbContext ){
        var EntryObj = new ObjModels.Entry { EntryId = newEntry.EntryId,
                                                SourceId = newEntry.SourceId,
                                                CIRId = newEntry.CIRId,
                                                IdInSource = newEntry.IdInSource,
                                                Name = newEntry.Name,
                                                Description = newEntry.Description,
                                                Inactive = newEntry.Inactive,
                                                CategoryRefId = newEntry.CategoryRefId,
                                                RegistryRefId = newEntry.RegistryRefId,
                                                SourceRefId = newEntry.SourceRefId,
                                                CreatedBy = "authUser",
                                                DateCreated = DateTime.UtcNow,
                                                ModifiedBy = "authUser",
                                                DateModified = DateTime.UtcNow 
                                                };    
        DbContext.Entry.Add(EntryObj);
        DbContext.SaveChanges();
    }
    public void UpdateEntry(string Id, EntryViewModel updateRegistry, CIRLibContext DbContext ){
        var EntryObj = DbContext.Entry.Where(item => item.Id.Equals(Id)).First();
        //TO DO : Have to update only those fields that were modified not all.
        EntryObj.EntryId = updateRegistry.EntryId;
        EntryObj.SourceId = updateRegistry.SourceId;
        EntryObj.CIRId = updateRegistry.CIRId;
        EntryObj.IdInSource = updateRegistry.IdInSource;
        EntryObj.Name = updateRegistry.Name;
        EntryObj.Description = updateRegistry.Description;
        EntryObj.Inactive = updateRegistry.Inactive;
        EntryObj.CategoryRefId = updateRegistry.CategoryRefId;
        EntryObj.RegistryRefId = updateRegistry.RegistryRefId;
        EntryObj.SourceRefId = updateRegistry.SourceRefId;
        EntryObj.CreatedBy = "authUser";
        EntryObj.DateCreated = DateTime.UtcNow;
        EntryObj.ModifiedBy = "authUser";
        EntryObj.DateModified = DateTime.UtcNow;
        DbContext.SaveChanges();
    }
    public void DeleteEntryById(string Id, CIRLibContext DbContext)
    {    
       var DelRegObj = DbContext.Entry.Where(item => item.Id.Equals(Id)).First(); 
       DbContext.Entry.Remove(DelRegObj);
       DbContext.SaveChanges();
    }
}
}