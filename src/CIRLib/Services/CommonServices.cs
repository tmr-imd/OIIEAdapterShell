using CIRLib.Persistence;
using System.Security.Claims;
using DataModel = DataModelServices;
namespace CIRServices;
public class CommonServices
{
    public static void checkMandatoryPropertiesPassed(DataModel.EntryDef newEntryObj = null,
    DataModel.RegistryDef? newRegObj = null, DataModel.CategoryDef? newCatObj = null, string? action = "")
    {
        if(string.IsNullOrWhiteSpace(newEntryObj.IdInSource) || string.IsNullOrWhiteSpace(newEntryObj.SourceId))
        {
            throw new Exception("Mandatory Entry Identifiers not provided. ");
        }

        if(action.Equals("add"))
        {
            if ((string.IsNullOrWhiteSpace(newRegObj.RegistryId) &&
                string.IsNullOrWhiteSpace(newEntryObj.RegistryRefId)) ||
                (string.IsNullOrWhiteSpace(newCatObj.CategoryId) &&
                string.IsNullOrWhiteSpace(newEntryObj.CategoryRefId))
                )
            {
                throw new Exception("Mandatory Entry Identifiers not provided. ");
            }
        }
    }

    public static bool CheckIfRegistryExists(string registryId, CIRLibContext dbContext, string action)
    {
        try
        {
            var regObj = dbContext.Registry.Where(item => item.RegistryId.Equals(registryId)).Count();
            if(regObj == 0)
            {
                if(action == "update")
                {
                    throw new Exception("Please provide a valid Registry Id.");
                }
                else if(action == "create")
                {
                    return false;
                }
            }
            return true;
        }
        catch(Exception ex)
        {
            throw new Exception(" Exception: "+ex);
        }
    }
    public static bool CheckIfCategoryExists(string categoryId, CIRLibContext dbContext, string action)
    {
        try
        {
            var catObj = dbContext.Category.Where(item => item.CategoryId.Equals(categoryId)).First();
            if(catObj == null)
            {
                if(action == "update")
                {
                    throw new Exception("Please provide a valid Category Id.");
                }
                else if(action == "create")
                {
                    return false;
                }
            }
            return true;
        }
        catch(Exception ex)
        {
            throw new Exception("Please enter a valid Category Id. "+ ex);
        }
    }
    public static void CheckIfEntryExists(string IdInSource, CIRLibContext DbContext)
    {
        try
        {
            var EntryObj = DbContext.Entry.Where(item => item.IdInSource.Equals(IdInSource)).First();
            if( EntryObj == null)
            {
                throw new Exception("Please enter a valid Entry Id.");
            }
        }
        catch(Exception ex)
        {
            throw new Exception("Please enter a valid Entry Id. "+ ex);
        }
    }

    public static void CheckIfPropertyExists(string PropertyId, CIRLibContext DbContext)
    {
        try
        {
            var PropertyObj = DbContext.Property.Where(item => item.PropertyId.Equals(PropertyId)).First();
            if( PropertyObj == null)
            {
                throw new Exception("Please enter a valid Property Id.");
            }
        }
        catch(Exception ex)
        {
            throw new Exception("Please enter a valid Property Id. "+ex);
        }
    }
}