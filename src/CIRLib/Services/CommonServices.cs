using CIRLib.Persistence;
using System.Security.Claims;

namespace CIRServices;
public class CommonServices{
    public static void CheckIfRegistryExists(string RegistryId, CIRLibContext DbContext){
        try{
            var RegObj = DbContext.Registry.Where(item => item.RegistryId.Equals(RegistryId)).First();
            if( RegObj == null){
                throw new Exception("Please enter a valid Registry Id.");
            }
        }
        catch(Exception ex){
            throw new Exception("Please enter a valid Registry Id. "+ex);
        }
    }
    public static void CheckIfCategoryExists(string CategoryId, CIRLibContext DbContext){
        try{
            var CatObj = DbContext.Category.Where(item => item.CategoryId.Equals(CategoryId)).First();
            if( CatObj == null){
                throw new Exception("Please enter a valid Category Id.");
            }
        }
        catch(Exception ex){
            throw new Exception("Please enter a valid Category Id. "+ex);
        }
    }
    public static void CheckIfEntryExists(string IdInSource, CIRLibContext DbContext){
        try{
            var EntryObj = DbContext.Entry.Where(item => item.IdInSource.Equals(IdInSource)).First();
            if( EntryObj == null){
                throw new Exception("Please enter a valid Entry Id.");
            }
        }
        catch(Exception ex){
            throw new Exception("Please enter a valid Entry Id. "+ex);
        }
    }

    public static void CheckIfPropertyExists(string PropertyId, CIRLibContext DbContext){
        try{
            var PropertyObj = DbContext.Property.Where(item => item.PropertyId.Equals(PropertyId)).First();
            if( PropertyObj == null){
                throw new Exception("Please enter a valid Property Id.");
            }
        }
        catch(Exception ex){
            throw new Exception("Please enter a valid Property Id. "+ex);
        }
    }
}