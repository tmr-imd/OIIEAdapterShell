
using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using CIRLIB.UI.Pages;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;

namespace CIRLib.UI.Services;
public class CategoryServices{
    public List<ObjModels.Category> GetAllCategories(CIRLibContext DbContext)
    {     
        return DbContext.Category.OrderByDescending(x => x.DateCreated).ToList();
    }
    public ObjModels.Category GetCategoryById(Guid Id, CIRLibContext DbContext)
    {     
        return DbContext.Category.Where(item => item.Id.Equals(Id)).First(); 
    }
    public void CreateNewCategory(CategoryViewModel NewCategory, CIRLibContext DbContext ){
        CommonServices.CheckIfRegistryExists(NewCategory.RegistryRefId, DbContext);
        var CategoryObj = new ObjModels.Category
        {
            CategoryId = NewCategory.CategoryId,       
            SourceId = NewCategory.SourceId,
            RegistryRefId = NewCategory.RegistryRefId,
            Description = NewCategory.Description
        };
        DbContext.Category.Add(CategoryObj);
        DbContext.SaveChanges();
    }
    public void UpdateCategory(Guid Id, CategoryViewModel UpdateCategory, CIRLibContext DbContext ){
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