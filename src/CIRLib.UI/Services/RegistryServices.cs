using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using CIRLIB.UI.Pages;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;

namespace CIRLib.UI.Services{
public class RegistryServices{
   
    public ObjModels.Registry GetRegistryById(Guid Id, CIRLibContext DbContext)
    {     
        return DbContext.Registry.Where(item => item.Id.Equals(Id)).First(); 
    }

    public List<ObjModels.Registry> GetRegistryFromFilters(string EntryId, string RegistryId, string CategoryId,
        string PropertyId, string PropertyValueKey, CIRLibContext DbContext)
    {   
        IQueryable<ObjModels.Registry> Query = DbContext.Registry;

        if(!string.IsNullOrWhiteSpace(CategoryId))
        {
            Query = Query.Join(
                DbContext.Category, 
                r => r.RegistryId,
                c => c.RegistryRefId,
                (r,c) => new 
                {
                    Registry = r,
                    Category = c
                }
            ).Where(
                joinResult => joinResult.Category.CategoryId == CategoryId
            )
            .Select(
                joinResult => joinResult.Registry
            );
        }

        if(!string.IsNullOrWhiteSpace(EntryId))
        {
            Query = Query.Join(
                DbContext.Entry, 
                r => r.RegistryId,
                e => e.RegistryRefId,
                (r,e) => new 
                {
                    Registry = r,
                    Entry = e
                }
            ).Where(
                joinResult => joinResult.Entry.IdInSource == EntryId
            )
            .Select(
                joinResult => joinResult.Registry
            );
        }

        if(!string.IsNullOrWhiteSpace(PropertyId))
        {
            if(string.IsNullOrWhiteSpace(PropertyValueKey))
            {
                Query = Query.Join(
                    DbContext.Property, 
                    r => r.RegistryId,
                    p => p.RegistryRefId,
                    (r,p) => new 
                    {
                        Registry = r,
                        Property = p
                    }
                ).Where(
                    joinResult => joinResult.Property.PropertyId == PropertyId
                ).Select(
                    joinResult => joinResult.Registry
                ); 
            }
            else
            {   
                //Property and PropertyValue Filters are used.
                Query = Query.Join(
                    DbContext.Property, 
                    r => r.RegistryId,
                    p => p.RegistryRefId,
                    (r,p) => new 
                    {
                        Registry = r,
                        Property = p
                    }
                ).Join(
                    DbContext.PropertyValue, 
                    p => p.Property.PropertyId,
                    pv => pv.PropertyRefId,
                    (p,pv) => new 
                    {
                        Registry = p.Registry,
                        Property = p.Property,
                        PropertyValue = pv
                    }
                ).Where(
                    joinResult => joinResult.PropertyValue.Key == PropertyValueKey
                ).Where(
                    joinResult => joinResult.Property.PropertyId == PropertyId
                ).Select(
                    joinResult => joinResult.Registry
                );           
            }
        }
        else if(string.IsNullOrWhiteSpace(PropertyId) && !string.IsNullOrWhiteSpace(PropertyValueKey))
        {   
            //Only the PropertyValue filter is used.
            Query = Query.Join(
                DbContext.Property, 
                r => r.RegistryId,
                p => p.RegistryRefId,
                (r,p) => new 
                {
                    Registry = r,
                    Property = p
                }
            ).Join(
                DbContext.PropertyValue, 
                p => p.Property.PropertyId,
                pv => pv.PropertyRefId,
                (p,pv) => new 
                {
                    Registry = p.Registry,
                    Property = p.Property,
                    PropertyValue = pv
                }
            ).Where(
                joinResult => joinResult.PropertyValue.Key == PropertyValueKey
            ).Select(
                joinResult => joinResult.Registry
            );           
        }

        if(!string.IsNullOrWhiteSpace(RegistryId))
        {
            Query = Query.Where(
                t => t.RegistryId == RegistryId
            );
        }
        return Query.ToList();
    }

    public void CreateNewRegistry( RegistryViewModel newRegistry, CIRLibContext DbContext )
    {
        var RegistryObj = new ObjModels.Registry 
        { 
            RegistryId = newRegistry.RegistryId,
            Description = newRegistry.Description,
            Id = Guid.NewGuid()
        };
        DbContext.Registry.Add(RegistryObj);
        DbContext.SaveChanges();
    }
    public void UpdateRegistry(Guid Id, RegistryViewModel updateRegistry, CIRLibContext DbContext )
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