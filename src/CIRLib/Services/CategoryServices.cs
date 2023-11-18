
using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using ObjModels = CIRLib.ObjectModel.Models;
using System;
using System.Security.Claims;
using Microsoft.Data.Sqlite;

namespace CIRServices;
public class CategoryServices : CommonServices
{
    public List<ObjModels.Category> GetAllCategories(CIRLibContext DbContext)
    {     
        return DbContext.Category.OrderByDescending(x => x.DateCreated).ToList();
    }

    public ObjModels.Category GetCategoryById(Guid Id, CIRLibContext DbContext)
    {     
        return DbContext.Category.Where(item => item.Id.Equals(Id)).First(); 
    }
    
    public List<ObjModels.Category> GetCategoryFromFilters(string entryId, string registryId, string categoryId,
        string propertyId, string propertyValueKey, CIRLibContext dbContext)
    {   
        IQueryable<ObjModels.Category> Query = dbContext.Category;

        if(!string.IsNullOrWhiteSpace(registryId))
        {
            Query = Query.Join(
                dbContext.Registry, 
                c => c.RegistryRefId,                
                r => r.Id,
                (c,r) => new 
                {
                    Category = c,
                    Registry = r
                }
            ).Where(
                joinResult => joinResult.Registry.RegistryId == registryId
            )
            .Select(
                joinResult => joinResult.Category
            );
        }

        if(!string.IsNullOrWhiteSpace(entryId))
        {
            Query = Query.Join(
                dbContext.Entry, 
                c => c.Id,
                e => e.CategoryRefId,
                (c,e) => new 
                {
                    Category = c,
                    Entry = e
                }
            ).Where(
                joinResult => joinResult.Entry.IdInSource == entryId
            )
            .Select(
                joinResult => joinResult.Category
            );
        }        

        if(!string.IsNullOrWhiteSpace(propertyId))
        {
            if(string.IsNullOrWhiteSpace(propertyValueKey))
            {
                Query = Query.Join(
                    dbContext.Property, 
                    c => c.Id,
                    p => p.Entry.CategoryRefId,
                    (c,p) => new 
                    {
                        Category = c,
                        Property = p
                    }
                ).Where(
                    joinResult => joinResult.Property.PropertyId == propertyId
                ).Select(
                    joinResult => joinResult.Category
                ); 
            }
            else
            {
                Query = Query.Join(
                    dbContext.Property, 
                    c => c.Id,
                    p => p.Entry.CategoryRefId,
                    (c,p) => new 
                    {
                        Category = c,
                        Property = p
                    }
                ).Join(
                    dbContext.PropertyValue, 
                    p => p.Property.Id,
                    pv => pv.PropertyRefId,
                    (p,pv) => new 
                    {
                        Category = p.Category,
                        Property = p.Property,
                        PropertyValue = pv
                    }
                ).Where(
                    joinResult => joinResult.PropertyValue.Key == propertyValueKey
                ).Where(
                    joinResult => joinResult.Property.PropertyId == propertyId
                ).Select(
                    joinResult => joinResult.Category
                );           
            }
        }
        else if(string.IsNullOrWhiteSpace(propertyId) && !string.IsNullOrWhiteSpace(propertyValueKey))
        {
            Query = Query.Join(
                dbContext.Property, 
                c => c.Id,
                p => p.Entry.CategoryRefId,
                (c,p) => new 
                {
                    Category = c,
                    Property = p
                }
            ).Join(
                dbContext.PropertyValue, 
                p => p.Property.Id,
                pv => pv.PropertyRefId,
                (p,pv) => new 
                {
                    Category = p.Category,
                    Property = p.Property,
                    PropertyValue = pv
                }
            ).Where(
                joinResult => joinResult.PropertyValue.Key == propertyValueKey
            ).Select(
                joinResult => joinResult.Category
            );           
        }
        
        if(!string.IsNullOrWhiteSpace(categoryId))
        {
            Query = Query.Where(
                joinResult => joinResult.CategoryId == categoryId
                );
        }
        return Query.ToList();
    }

    public ObjModels.Category CreateNewCategory(ObjModels.Category newCategory, CIRLibContext dbContext )
    {
        var registry = CheckIfRegistryExists(newCategory.RegistryId, dbContext, "create");
        if(registry == null)
        {
            // If Registry does not exists, we create one
            registry = new ObjModels.Registry()
            {
                RegistryId = newCategory.RegistryId,
                Id = Guid.NewGuid()
            };
            dbContext.Registry.Add(registry);
        }
        newCategory.Registry = registry;

        newCategory.Id = Guid.NewGuid();
        dbContext.Category.Add(newCategory);
        dbContext.SaveChanges();
        return newCategory;
    }

    public void UpdateCategory(Guid id, ObjModels.Category updateCategory, CIRLibContext dbContext )
    {
        var category = dbContext.Category.Find(id);
        if (category is null) return;

        category.Description = updateCategory.Description;
        dbContext.SaveChanges();
    }
    
    public void DeleteCategoryById(Guid id, CIRLibContext dbContext)
    {    
       var DelCategoryObj = dbContext.Category.Where(item => item.Id.Equals(id)).First(); 
       dbContext.Category.Remove(DelCategoryObj);
       dbContext.SaveChanges();
    }

}