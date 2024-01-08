using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRServices;
public class EntryServices : CommonServices
{
    public List<ObjModels.Entry> GetAllEntries(CIRLibContext DbContext)
    {
        return DbContext.Entry.OrderByDescending(x => x.DateCreated).ToList();
    }
    public ObjModels.Entry GetEntryById(Guid Id, CIRLibContext DbContext)
    {
        return DbContext.Entry.Where(item => item.Id.Equals(Id)).First();
    }

    public List<ObjModels.Entry> GetEntriesFromFilters(
        string entryId = "", string entrySourceId = "", string registryId = "", 
        string categoryId = "", string categorySourceID = "", string propertyId = "",
        string propertyValueKey = "", string CIRId = "", string parentEntityId = "",
        CIRLibContext dbContext = null!)
    {
        IQueryable<ObjModels.Entry> Query = dbContext.Entry;

        if(string.IsNullOrWhiteSpace(registryId) && string.IsNullOrWhiteSpace(categoryId) &&
         string.IsNullOrWhiteSpace(entryId) && string.IsNullOrWhiteSpace(entrySourceId) &&
         string.IsNullOrWhiteSpace(propertyId) && string.IsNullOrWhiteSpace(propertyValueKey) &&
         string.IsNullOrWhiteSpace(CIRId) && string.IsNullOrWhiteSpace(categorySourceID) &&
         string.IsNullOrWhiteSpace(parentEntityId))
        {   
            //If none of the filters are present we return all records.
            return Query.ToList();
        }

        if (!string.IsNullOrWhiteSpace(registryId))
        {
            Query = Query.Join(
                dbContext.Registry,
                e => e.RegistryRefId,
                r => r.Id,
                (e, r) => new
                {
                    Entry = e,
                    Registry = r
                }
                ).Where(
                    joinResult => joinResult.Registry.RegistryId.Contains(registryId)
                )
                .Select(
                    joinResult => joinResult.Entry
                );
        }

        if (!string.IsNullOrWhiteSpace(categoryId) || !string.IsNullOrWhiteSpace(categorySourceID))
        {
            var TempQuery = Query.Join(
                dbContext.Category,
                e => e.CategoryRefId,
                c => c.Id,
                (e, c) => new
                {
                    Entry = e,
                    Category = c
                }
                );
            if (!string.IsNullOrWhiteSpace(categoryId))
            {
                TempQuery = TempQuery.Where(
                    joinResult => joinResult.Category.CategoryId.Contains(categoryId)
                );
            }
            if (!string.IsNullOrWhiteSpace(categorySourceID))
            {
                TempQuery = TempQuery.Where(
                    joinResult => joinResult.Category.CategorySourceId.Contains(categorySourceID)
                );
            }
            Query = TempQuery.Select(
                    joinResult => joinResult.Entry
                );
        }

        if (!string.IsNullOrWhiteSpace(propertyId))
        {
            if (string.IsNullOrWhiteSpace(propertyValueKey))
            {
                Query = Query.Join(
                    dbContext.Property,
                    e => e.Id,
                    p => p.EntryRefId,
                    (e, p) => new
                    {
                        Entry = e,
                        Property = p
                    }
                    ).Where(
                        joinResult => joinResult.Property.PropertyId.Contains(propertyId)
                    ).Select(
                        joinResult => joinResult.Entry
                    );
            }
            else
            {
                Query = Query.Join(
                    dbContext.Property,
                    e => e.Id,
                    p => p.EntryRefId,
                    (e, p) => new
                    {
                        Entry = e,
                        Property = p
                    }
                    ).Join(
                        dbContext.PropertyValue,
                        p => p.Property.Id,
                        pv => pv.PropertyRefId,
                        (p, pv) => new
                        {
                            Entry = p.Entry,
                            Property = p.Property,
                            PropertyValue = pv
                        }
                    ).Where(
                        joinResult => joinResult.PropertyValue.Key.Contains(propertyValueKey)
                    ).Where(
                        joinResult => joinResult.Property.PropertyId.Contains(propertyId)
                    ).Select(
                        joinResult => joinResult.Entry
                    );
            }
        }
        else if (string.IsNullOrWhiteSpace(propertyId) && !string.IsNullOrWhiteSpace(propertyValueKey))
        {
            Query = Query.Join
                (
                    dbContext.Property,
                    e => e.Id,
                    p => p.EntryRefId,
                    (e, p) => new
                    {
                        Entry = e,
                        Property = p
                    }
                )
                .Join
                (
                    dbContext.PropertyValue,
                    p => p.Property.Id,
                    pv => pv.PropertyRefId,
                    (p, pv) => new
                    {
                        Entry = p.Entry,
                        Property = p.Property,
                        PropertyValue = pv
                    }
                ).Where
                (
                    joinResult => joinResult.PropertyValue.Key.Contains(propertyValueKey)
                ).Select
                (
                    joinResult => joinResult.Entry
                );
        }

        if (!string.IsNullOrWhiteSpace(entrySourceId))
        {
            Query = Query.Where
            (
                joinResult => joinResult.SourceId.Contains(entrySourceId)
            );
        }

        if (!string.IsNullOrWhiteSpace(parentEntityId))
        {
            Query = Query.Where(entry => entry.ParentEntityId.Contains(parentEntityId));
        }

        // Since CIRId takes precedence over entryId.
        // If CIRId is filtered, filter with only CIRId .
        if (!string.IsNullOrWhiteSpace(entryId) && !string.IsNullOrWhiteSpace(CIRId))
        {
            Query = Query.Where
            (
                joinResult => joinResult.CIRId.Contains(CIRId)
            );
        }
        //When CIRId is not filtered, filter with entryId if present.
        else if (!string.IsNullOrWhiteSpace(entryId) && string.IsNullOrWhiteSpace(CIRId))
        {
            Query = Query.Where
            (
                joinResult => joinResult.IdInSource.Contains(entryId)
            );
        }
        // Since CIRId takes precedence over entry Id
        else if (!string.IsNullOrWhiteSpace(CIRId) && string.IsNullOrWhiteSpace(entryId))
        {
            Query = Query.Where(
            joinResult => joinResult.CIRId.Contains(CIRId)
            );
        }

        return Query.ToList();
    }
    public void CreateNewEntry(ObjModels.Entry newEntry, CIRLibContext dbContext)
    {
        var registryObjExists = CheckIfRegistryExists(newEntry.RegistryId, dbContext, "create");
        if(registryObjExists == null)
        {
            //If Registry does not exists, we create one.
            var regObj = new ObjModels.Registry()
            {
                RegistryId = newEntry.RegistryId,
                Id = Guid.NewGuid()
            };
            dbContext.Registry.Add(regObj);
            newEntry.Registry = regObj;
            registryObjExists = regObj;
        }
        else
        {
            newEntry.Registry = registryObjExists;
        }

        var categoryObjExists = CheckIfCategoryExists(newEntry.CategoryId, dbContext, "create");
        if(categoryObjExists == null)
        {
            //If Category does not exists, we create one.
            var catObj = new ObjModels.Category()
            {
                CategoryId = newEntry.CategoryId,
                RegistryId = newEntry.RegistryId,
                Id = Guid.NewGuid()
            };
            catObj.Registry = registryObjExists;
            dbContext.Category.Add(catObj);
            newEntry.Category = catObj;
        }
        else
        {
            newEntry.Category = categoryObjExists;
        }
        
        newEntry.Id = Guid.NewGuid();
        dbContext.Entry.Add(newEntry);
        dbContext.SaveChanges();
    }

    public void UpdateEntry(Guid Id, ObjModels.Entry updateEntry, CIRLibContext dbContext = null!)
    {
        var EntryObj = dbContext.Entry.Where(item => item.Id.Equals(Id)).First();
        
        EntryObj.Name = updateEntry.Name;
        EntryObj.Description = updateEntry.Description;
        EntryObj.Inactive = updateEntry.Inactive;
        dbContext.SaveChanges();
    }
    
    public void DeleteEntryById(Guid Id, CIRLibContext DbContext)
    {
        var DelRegObj = DbContext.Entry.Where(item => item.Id.Equals(Id)).First();
        DbContext.Entry.Remove(DelRegObj);
        DbContext.SaveChanges();
    }

    public void UpdateCIRIdInEntry(string CIRId, ObjModels.Entry updateEntry, CIRLibContext dbContext = null!)
    {
        var EntryObj = dbContext.Entry.Where(item => item.Id.Equals(updateEntry.Id)).First();

        EntryObj.CIRId = CIRId;
        dbContext.SaveChanges();
    }
}