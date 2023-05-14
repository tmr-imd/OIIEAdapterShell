
using Microsoft.EntityFrameworkCore;
using CIRLib.Persistence;
using CIRLIB.UI.Pages;
using ObjModels = CIRLib.ObjectModel.Models;
using System.Security.Claims;

namespace CIRLib.UI;
public class CategoryServices{
    public List<ObjModels.Category> GetAllCategories(CIRLibContext DbContext)
    {     
        return DbContext.Category.OrderByDescending(x => x.DateCreated).ToList();
    }
    public ObjModels.Category GetCategoryById(string Id, CIRLibContext DbContext)
    {     
        return DbContext.Category.Where(item => item.Id.Equals(Id)).First(); 
    }
    public void CreateNewCategory(CategoryViewModel NewCategory, CIRLibContext DbContext ){
        var CategoryObj = new ObjModels.Category { CategoryId = NewCategory.CategoryId,       
                                                   SourceId = NewCategory.SourceId,
                                                   RegistryRefId = NewCategory.RegistryRefId,
                                                   Description = NewCategory.Description,
                                                   CreatedBy = "authUser",
                                                   DateCreated = DateTime.UtcNow,
                                                   ModifiedBy = "authUser",
                                                   DateModified = DateTime.UtcNow };
        DbContext.Category.Add(CategoryObj);
        DbContext.SaveChanges();
    }
    public void UpdateCategory(string Id, CategoryViewModel UpdateCategory, CIRLibContext DbContext ){
        var CategoryObj = DbContext.Category.Where(item => item.Id.Equals(Id)).First();
        CategoryObj.SourceId = UpdateCategory.SourceId;
        CategoryObj.RegistryRefId = UpdateCategory.RegistryRefId;
        CategoryObj.Description = UpdateCategory.Description;
        CategoryObj.ModifiedBy = "authUser";
        CategoryObj.DateModified = DateTime.UtcNow;
        DbContext.SaveChanges();
    }
    public void DeleteCategoryById(string Id, CIRLibContext DbContext)
    {    
       var DelCategoryObj = DbContext.Category.Where(item => item.Id.Equals(Id)).First(); 
       DbContext.Category.Remove(DelCategoryObj);
       DbContext.SaveChanges();
    }
}