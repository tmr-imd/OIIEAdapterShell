using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;


namespace CIRServices
{
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
            string propertyValueKey = "", string CIRId = "", CIRLibContext? dbContext=null)
        {
            IQueryable<ObjModels.Entry> Query = dbContext.Entry;

            if (!string.IsNullOrWhiteSpace(registryId))
            {
                Query = Query.Join(
                    dbContext.Registry,
                    e => e.RegistryRefId,
                    r => r.RegistryId,
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
                    c => c.CategoryId,
                    (e, c) => new
                    {
                        Entry = e,
                        Category = c
                    }
                    );
                if (!string.IsNullOrWhiteSpace(categoryId))
                {
                    TempQuery.Where(
                        joinResult => joinResult.Category.CategoryId.Contains(categoryId)
                    );
                }
                if (!string.IsNullOrWhiteSpace(categorySourceID))
                {
                    TempQuery.Where(
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
                        e => e.IdInSource,
                        p => p.EntryRefIdInSource,
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
                        e => e.IdInSource,
                        p => p.EntryRefIdInSource,
                        (e, p) => new
                        {
                            Entry = e,
                            Property = p
                        }
                        ).Join(
                            dbContext.PropertyValue,
                            p => p.Property.PropertyId,
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
                Query = Query.Join(
                        dbContext.Property,
                        e => e.IdInSource,
                        p => p.EntryRefIdInSource,
                        (e, p) => new
                        {
                            Entry = e,
                            Property = p
                        }
                    ).Join(
                        dbContext.PropertyValue,
                        p => p.Property.PropertyId,
                        pv => pv.PropertyRefId,
                        (p, pv) => new
                        {
                            Entry = p.Entry,
                            Property = p.Property,
                            PropertyValue = pv
                        }
                    ).Where(
                        joinResult => joinResult.PropertyValue.Key.Contains(propertyValueKey)
                    ).Select(
                        joinResult => joinResult.Entry
                    );
            }

            if (!string.IsNullOrWhiteSpace(entryId) || !string.IsNullOrWhiteSpace(entrySourceId) 
                || !string.IsNullOrWhiteSpace(CIRId))
            {
                if (!string.IsNullOrWhiteSpace(entryId))
                {
                    Query = Query.Where(
                        joinResult => joinResult.IdInSource.Contains(entryId)
                    );
                }
                if (!string.IsNullOrWhiteSpace(entrySourceId))
                {
                    Query = Query.Where(
                    joinResult => joinResult.SourceId.Contains(entrySourceId)
                    );
                }
                if (!string.IsNullOrWhiteSpace(CIRId))
                {
                    Query = Query.Where(
                    joinResult => joinResult.CIRId.Contains(CIRId)
                    );
                }
            }
            return Query.ToList();
        }
        public void CreateNewEntry(ObjModels.Entry newEntry, CIRLibContext dbContext)
        {
            var registryExists = CheckIfRegistryExists(newEntry.RegistryRefId, dbContext, "create");
            if(!registryExists)
            {
                //If Registry does not exists, we create one.
                var regObj = new ObjModels.Registry()
                {
                    RegistryId = newEntry.RegistryRefId,
                    Id = Guid.NewGuid()
                };
                dbContext.Registry.Add(regObj);
                dbContext.SaveChanges();
            }

            var categoryExists = CheckIfCategoryExists(newEntry.CategoryRefId, dbContext, "create");
            if(!categoryExists)
            {
                //If Category does not exists, we create one.
                var catObj = new ObjModels.Category()
                {
                    CategoryId = newEntry.CategoryRefId,
                    RegistryRefId = newEntry.RegistryRefId,
                    Id = Guid.NewGuid()
                };
                dbContext.Category.Add(catObj);
                dbContext.SaveChanges();
            }
            
            newEntry.Id = Guid.NewGuid();
            dbContext.Entry.Add(newEntry);
            dbContext.SaveChanges();
        }

        public void UpdateEntry(Guid Id, ObjModels.Entry updateEntry, CIRLibContext DbContext)
        {
            var EntryObj = DbContext.Entry.Where(item => item.Id.Equals(Id)).First();
            //TO DO : Have to update only those fields that were modified not all.
            EntryObj.Name = updateEntry.Name;
            EntryObj.Description = updateEntry.Description;
            EntryObj.Inactive = updateEntry.Inactive;
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