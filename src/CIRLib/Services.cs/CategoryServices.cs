
using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using ObjModels = CIRLib.ObjectModel.Models;
using System;
using System.Security.Claims;
using Microsoft.Data.Sqlite;

namespace CIRServices;
public class CategoryServices{

    public List<ObjModels.Category> GetAllCategories(CIRLibContext DbContext)
    {     
        return DbContext.Category.OrderByDescending(x => x.DateCreated).ToList();
    }

    public ObjModels.Category GetCategoryById(Guid Id, CIRLibContext DbContext)
    {     
        return DbContext.Category.Where(item => item.Id.Equals(Id)).First(); 
    }
    
    public List<ObjModels.Category> GetCategoryFromFilters(string EntryId, string RegistryId, string CategoryId,
        string PropertyId, string PropertyValueKey, CIRLibContext DbContext)
    {   
        IQueryable<ObjModels.Category> Query = DbContext.Category;

        if(!string.IsNullOrWhiteSpace(RegistryId))
        {
            Query = Query.Join(
                DbContext.Registry, 
                c => c.RegistryRefId,                
                r => r.RegistryId,
                (c,r) => new 
                {
                    Category = c,
                    Registry = r
                }
            ).Where(
                joinResult => joinResult.Registry.RegistryId == RegistryId
            )
            .Select(
                joinResult => joinResult.Category
            );
        }

        if(!string.IsNullOrWhiteSpace(EntryId))
        {
            Query = Query.Join(
                DbContext.Entry, 
                c => c.CategoryId,
                e => e.CategoryRefId,
                (c,e) => new 
                {
                    Category = c,
                    Entry = e
                }
            ).Where(
                joinResult => joinResult.Entry.IdInSource == EntryId
            )
            .Select(
                joinResult => joinResult.Category
            );
        }        

        if(!string.IsNullOrWhiteSpace(PropertyId))
        {
            if(string.IsNullOrWhiteSpace(PropertyValueKey))
            {
                Query = Query.Join(
                    DbContext.Property, 
                    c => c.CategoryId,
                    p => p.CategoryRefId,
                    (c,p) => new 
                    {
                        Category = c,
                        Property = p
                    }
                ).Where(
                    joinResult => joinResult.Property.PropertyId == PropertyId
                ).Select(
                    joinResult => joinResult.Category
                ); 
            }
            else
            {
                Query = Query.Join(
                    DbContext.Property, 
                    c => c.CategoryId,
                    p => p.CategoryRefId,
                    (c,p) => new 
                    {
                        Category = c,
                        Property = p
                    }
                ).Join(
                    DbContext.PropertyValue, 
                    p => p.Property.PropertyId,
                    pv => pv.PropertyRefId,
                    (p,pv) => new 
                    {
                        Category = p.Category,
                        Property = p.Property,
                        PropertyValue = pv
                    }
                ).Where(
                    joinResult => joinResult.PropertyValue.Key == PropertyValueKey
                ).Where(
                    joinResult => joinResult.Property.PropertyId == PropertyId
                ).Select(
                    joinResult => joinResult.Category
                );           
            }
        }
        else if(string.IsNullOrWhiteSpace(PropertyId) && !string.IsNullOrWhiteSpace(PropertyValueKey))
        {
            Query = Query.Join(
                DbContext.Property, 
                c => c.CategoryId,
                p => p.CategoryRefId,
                (c,p) => new 
                {
                    Category = c,
                    Property = p
                }
            ).Join(
                DbContext.PropertyValue, 
                p => p.Property.PropertyId,
                pv => pv.PropertyRefId,
                (p,pv) => new 
                {
                    Category = p.Category,
                    Property = p.Property,
                    PropertyValue = pv
                }
            ).Where(
                joinResult => joinResult.PropertyValue.Key == PropertyValueKey
            ).Select(
                joinResult => joinResult.Category
            );           
        }
        
        if(!string.IsNullOrWhiteSpace(CategoryId))
        {
            Query = Query.Where(
                joinResult => joinResult.CategoryId == CategoryId
                );
        }
        return Query.ToList();
    }

    public void CreateNewCategory(ObjModels.Category NewCategory, CIRLibContext DbContext )
    {
        CommonServices.CheckIfRegistryExists(NewCategory.RegistryRefId, DbContext);
        
        DbContext.Category.Add(NewCategory);
        DbContext.SaveChanges();
    }
    public void UpdateCategory(Guid Id, ObjModels.Category UpdateCategory, CIRLibContext DbContext )
    {
        var CategoryObj = DbContext.Category.Where(item => item.Id.Equals(Id)).First();
        CommonServices.CheckIfRegistryExists(UpdateCategory.RegistryRefId,DbContext);                   
        CategoryObj.SourceId = UpdateCategory.SourceId;
        CategoryObj.RegistryRefId = UpdateCategory.RegistryRefId;
        CategoryObj.Description = UpdateCategory.Description;
        DbContext.SaveChanges();

    }
    public void DeleteCategoryById(Guid Id, CIRLibContext DbContext)
    {    
       var DelCategoryObj = DbContext.Category.Where(item => item.Id.Equals(Id)).First(); 
       DbContext.Category.Remove(DelCategoryObj);
       DbContext.SaveChanges();
    }

}