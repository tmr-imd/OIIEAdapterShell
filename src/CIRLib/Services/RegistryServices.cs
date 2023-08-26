using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;

namespace CIRServices{
public class RegistryServices: CommonServices
{
   
    public ObjModels.Registry GetRegistryById(Guid Id, CIRLibContext dbContext)
    {     
        return dbContext.Registry.Where(item => item.Id.Equals(Id)).First(); 
    }

    public List<ObjModels.Registry> GetRegistryFromFilters(string entryId, string registryId, string categoryId,
        string propertyId, string propertyValueKey, CIRLibContext dbContext)
    {   
        IQueryable<ObjModels.Registry> Query = dbContext.Registry;

        if(!string.IsNullOrWhiteSpace(categoryId))
        {
            Query = Query.Join(
                dbContext.Category, 
                r => r.Id,
                c => c.RegistryRefId,
                (r,c) => new 
                {
                    Registry = r,
                    Category = c
                }
            ).Where(
                joinResult => joinResult.Category.CategoryId == categoryId
            )
            .Select(
                joinResult => joinResult.Registry
            );
        }

        if(!string.IsNullOrWhiteSpace(entryId))
        {
            Query = Query.Join(
                dbContext.Entry, 
                r => r.Id,
                e => e.RegistryRefId,
                (r,e) => new 
                {
                    Registry = r,
                    Entry = e
                }
            ).Where(
                joinResult => joinResult.Entry.IdInSource == entryId
            )
            .Select(
                joinResult => joinResult.Registry
            );
        }

        if(!string.IsNullOrWhiteSpace(propertyId))
        {
            if(string.IsNullOrWhiteSpace(propertyValueKey))
            {
                Query = Query.Join(
                    dbContext.Property, 
                    r => r.Id,
                    p => p.Entry.RegistryRefId,
                    (r,p) => new 
                    {
                        Registry = r,
                        Property = p
                    }
                ).Where(
                    joinResult => joinResult.Property.PropertyId == propertyId
                ).Select(
                    joinResult => joinResult.Registry
                ); 
            }
            else
            {   
                //Property and PropertyValue Filters are used.
                Query = Query.Join(
                    dbContext.Property, 
                    r => r.Id,
                    p => p.Entry.RegistryRefId,
                    (r,p) => new 
                    {
                        Registry = r,
                        Property = p
                    }
                ).Join(
                    dbContext.PropertyValue, 
                    p => p.Property.Id,
                    pv => pv.PropertyRefId,
                    (p,pv) => new 
                    {
                        Registry = p.Registry,
                        Property = p.Property,
                        PropertyValue = pv
                    }
                ).Where(
                    joinResult => joinResult.PropertyValue.Key == propertyValueKey
                ).Where(
                    joinResult => joinResult.Property.PropertyId == propertyId
                ).Select(
                    joinResult => joinResult.Registry
                );           
            }
        }
        else if(string.IsNullOrWhiteSpace(propertyId) && !string.IsNullOrWhiteSpace(propertyValueKey))
        {   
            //Only the PropertyValue filter is used.
            Query = Query.Join(
                dbContext.Property, 
                r => r.Id,
                p => p.Entry.RegistryRefId,
                (r,p) => new 
                {
                    Registry = r,
                    Property = p
                }
            ).Join(
                dbContext.PropertyValue, 
                p => p.Property.Id,
                pv => pv.PropertyRefId,
                (p,pv) => new 
                {
                    Registry = p.Registry,
                    Property = p.Property,
                    PropertyValue = pv
                }
            ).Where(
                joinResult => joinResult.PropertyValue.Key == propertyValueKey
            ).Select(
                joinResult => joinResult.Registry
            );           
        }

        if(!string.IsNullOrWhiteSpace(registryId))
        {
            Query = Query.Where(
                t => t.RegistryId == registryId
            );
        }
        return Query.ToList();
    }

    public void CreateNewRegistry(ObjModels.Registry RegistryObj, CIRLibContext dbContext )
    {   
        var registryExists = CheckIfRegistryExists(RegistryObj.RegistryId, dbContext, "create");
        if(registryExists == null)
        {
            RegistryObj.Id = Guid.NewGuid();
            dbContext.Registry.Add(RegistryObj);
            dbContext.SaveChanges();
        }
        else
        {
            throw new Exception("Registry exists in CIR Cache.");
        }
    }
    public void UpdateRegistry(Guid Id, ObjModels.Registry updateRegistry, CIRLibContext DbContext )
    {
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