using CIRLib.Persistence;
using System.Security.Claims;
using DataModel = DataModelServices;
using ObjModels = CIRLib.ObjectModel.Models;
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
                throw new Exception("Mandatory Entry Reference Identifiers not provided. ");
            }
        }
    }

    public static bool CheckIfRegistryExists(string registryId, CIRLibContext dbContext, string action)
    {
        try
        {
            if(string.IsNullOrWhiteSpace(registryId))
            {
                throw new Exception("Please provide a valid RegistryId.");
            }
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
            if(string.IsNullOrWhiteSpace(categoryId))
            {
                throw new Exception("Please provide a valid CategoryId.");
            }
            var catObj = dbContext.Category.Where(item => item.CategoryId.Equals(categoryId)).Count();
            if(catObj == 0)
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
            throw new Exception("Exception: "+ ex);
        }
    }
    public static ObjModels.Entry CheckIfEntryExists(string idInSource, CIRLibContext dbContext, string action="")
    {
        try
        {
            if(string.IsNullOrWhiteSpace(idInSource))
            {
                throw new Exception("Please provide a valid EntryId.");
            }

            var entryObj = dbContext.Entry.Where(item => item.IdInSource.Equals(idInSource)).ToList();
            if(entryObj.Count() == 0)
            {
                if(action == "update")
                {
                    throw new Exception("Please provide a valid EntryId.");
                }
                else if(action == "create")
                {
                    return null;
                }
            }
            else
            {
                return entryObj.First();
            }
            return null;
        }
        catch(Exception ex)
        {
            throw new Exception("Exception: "+ ex);
        }
    }

    public static bool CheckIfPropertyExists(string propertyId, CIRLibContext dbContext, string action="")
    {
        try
        {
            if(string.IsNullOrWhiteSpace(propertyId))
            {
                throw new Exception("Please provide a valid PropertyId.");
            }
            var propertyObj = dbContext.Property.Where(item => item.PropertyId.Equals(propertyId)).Count();
            if(propertyObj == 0)
            {
                if(action == "update")
                {
                    throw new Exception("Please provide a valid EntryId.");
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
            throw new Exception("Exception: "+ex);
        }
    }
}