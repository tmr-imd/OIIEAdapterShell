using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using CIRLIB.UI.Pages;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRLib.UI.Services{
public class EntryServices{
    
    public List<ObjModels.Entry> GetAllEntries(CIRLibContext DbContext)
    {     
        return DbContext.Entry.OrderByDescending(x => x.DateCreated).ToList();
    }
    public ObjModels.Entry GetEntryById(Guid Id, CIRLibContext DbContext)
    {     
        return DbContext.Entry.Where(item => item.Id.Equals(Id)).First(); 
    }
    public List<ObjModels.Entry> GetEntriesFromFilters(string RegistryId, string CategoryId, string EntryId,
        string PropertyId, string PropertyValueKey, CIRLibContext DbContext)
    {     
       IQueryable<ObjModels.Entry> Query = DbContext.Entry;

        if(!string.IsNullOrWhiteSpace(RegistryId))
        {
            Query = Query.Join(
                DbContext.Registry, 
                e => e.RegistryRefId,                
                r => r.RegistryId,
                (e,r) => new 
                {
                    Entry = e,
                    Registry = r
                }
                ).Where(
                    joinResult => joinResult.Registry.RegistryId == RegistryId
                )
                .Select(
                    joinResult => joinResult.Entry
                );
        }

        if(!string.IsNullOrWhiteSpace(CategoryId))
        {
            Query = Query.Join(
                DbContext.Category,                
                e => e.CategoryRefId,
                c => c.CategoryId,
                (e,c) => new 
                {                    
                    Entry = e,
                    Category = c
                }
                ).Where(
                    joinResult => joinResult.Category.CategoryId == CategoryId
                ).Select(
                    joinResult => joinResult.Entry
                );
        }        

        if(!string.IsNullOrWhiteSpace(PropertyId))
        {
            if(string.IsNullOrWhiteSpace(PropertyValueKey))
            {
                Query = Query.Join(
                    DbContext.Property, 
                    e => e.IdInSource,
                    p => p.EntryRefIdInSource,
                    (e,p) => new 
                    {
                        Entry = e,
                        Property = p
                    }
                    ).Where(
                        joinResult => joinResult.Property.PropertyId == PropertyId
                    ).Select(
                        joinResult => joinResult.Entry
                    ); 
            }
            else
            {
                Query = Query.Join(
                    DbContext.Property, 
                    e => e.IdInSource,
                    p => p.EntryRefIdInSource,
                    (e,p) => new 
                    {
                        Entry = e,
                        Property = p
                    }
                    ).Join(
                        DbContext.PropertyValue, 
                        p => p.Property.PropertyId,
                        pv => pv.PropertyRefId,
                        (p,pv) => new 
                        {
                            Entry = p.Entry,
                            Property = p.Property,
                            PropertyValue = pv
                        }
                    ).Where(
                        joinResult => joinResult.PropertyValue.Key == PropertyValueKey
                    ).Where(
                        joinResult => joinResult.Property.PropertyId == PropertyId
                    ).Select(
                        joinResult => joinResult.Entry
                    );           
            }
        }
        else if(string.IsNullOrWhiteSpace(PropertyId) && !string.IsNullOrWhiteSpace(PropertyValueKey))
        {
            Query = Query.Join(
                    DbContext.Property, 
                    e => e.IdInSource,
                    p => p.EntryRefIdInSource,
                    (e,p) => new 
                    {
                        Entry = e,
                        Property = p
                    }
                ).Join(
                    DbContext.PropertyValue, 
                    p => p.Property.PropertyId,
                    pv => pv.PropertyRefId,
                    (p,pv) => new 
                    {
                        Entry = p.Entry,
                        Property = p.Property,
                        PropertyValue = pv
                    }
                ).Where(
                    joinResult => joinResult.PropertyValue.Key == PropertyValueKey
                ).Select(
                    joinResult => joinResult.Entry
                );           
        }
        
        if(!string.IsNullOrWhiteSpace(EntryId))
        {
            Query = Query.Where(
                joinResult => joinResult.IdInSource == EntryId
                );
        }
        return Query.ToList();
        
    }
    public void CreateNewEntry( EntryViewModel newEntry, CIRLibContext DbContext )
    {
        CommonServices.CheckIfRegistryExists(newEntry.RegistryRefId, DbContext);
        CommonServices.CheckIfCategoryExists(newEntry.CategoryRefId, DbContext);

        var EntryObj = new ObjModels.Entry
        {
            IdInSource = newEntry.IdInSource,
            SourceId = newEntry.SourceId,
            CIRId = newEntry.CIRId,
            SourceOwnerId = newEntry.SourceOwnerId,
            Name = newEntry.Name,
            Description = newEntry.Description,
            Inactive = newEntry.Inactive,
            CategoryRefId = newEntry.CategoryRefId,
            RegistryRefId = newEntry.RegistryRefId,
            Id = Guid.NewGuid()
        };    
        DbContext.Entry.Add(EntryObj);
        DbContext.SaveChanges();
    }
        
    public void UpdateEntry(Guid Id, EntryViewModel updateEntry, CIRLibContext DbContext )
    {
        CommonServices.CheckIfRegistryExists(updateEntry.RegistryRefId, DbContext);
        CommonServices.CheckIfCategoryExists(updateEntry.CategoryRefId, DbContext);

        var EntryObj = DbContext.Entry.Where(item => item.Id.Equals(Id)).First();
        //TO DO : Have to update only those fields that were modified not all.
        EntryObj.IdInSource = updateEntry.IdInSource;
        EntryObj.SourceId = updateEntry.SourceId;
        EntryObj.CIRId = updateEntry.CIRId;
        EntryObj.SourceOwnerId = updateEntry.SourceOwnerId;
        EntryObj.Name = updateEntry.Name;
        EntryObj.Description = updateEntry.Description;
        EntryObj.Inactive = updateEntry.Inactive;
        EntryObj.CategoryRefId = updateEntry.CategoryRefId;
        EntryObj.RegistryRefId = updateEntry.RegistryRefId;
        DbContext.SaveChanges();
    }
    public void DeleteEntryById(Guid Id, CIRLibContext DbContext)
    {    
       var DelRegObj = DbContext.Entry.Where(item => item.Id.Equals(Id)).First(); 
       DbContext.Entry.Remove(DelRegObj);
       DbContext.SaveChanges();
    }
    }
}