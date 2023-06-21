using CIRLib.Persistence;
using System.Security.Claims;
using DataModel = DataModelServices;
using ObjModels = CIRLib.ObjectModel.Models;

namespace CIRServices;

public class CommonServices
{
    public static ObjModels.Registry? CheckIfRegistryExists(string registryId, CIRLibContext dbContext, string action)
    {
        try
        {
            if(string.IsNullOrWhiteSpace(registryId))
            {
                throw new Exception("Please provide a valid RegistryId.");
            }
            var regObj = dbContext.Registry.FirstOrDefault(item => item.RegistryId.Equals(registryId));
            if(regObj == null)
            {
                if(action == "update")
                {
                    throw new Exception("Please provide a valid Registry Id.");
                }
                else if(action == "create")
                {
                    return regObj;
                }
            }
            return regObj;
        }
        catch(Exception ex)
        {
            throw new Exception(" Exception: "+ex);
        }
    }
    public static ObjModels.Category? CheckIfCategoryExists(string categoryId, CIRLibContext dbContext, string action)
    {
        try
        {
            if(string.IsNullOrWhiteSpace(categoryId))
            {
                throw new Exception("Please provide a valid CategoryId.");
            }
            var catObj = dbContext.Category.SingleOrDefault(item => item.CategoryId.Equals(categoryId));
            if(catObj == null)
            {
                if(action == "update")
                {
                    throw new Exception("Please provide a valid Category Id.");
                }
                else if(action == "create")
                {
                    return catObj;
                }
            }
            return catObj;
        }
        catch(Exception ex)
        {
            throw new Exception("Exception: "+ ex);
        }
    }
    public static ObjModels.Entry? CheckIfEntryExists(string idInSource, CIRLibContext dbContext,
        string action="")
    {
        try
        {
            if(string.IsNullOrWhiteSpace(idInSource))
            {
                throw new ArgumentException("Please provide a valid EntryId.");
            }

            var entryObj = dbContext.Entry.SingleOrDefault(item => item.IdInSource.Equals(idInSource));
            if(entryObj == null)
            {
                if(action == "update")
                {
                    throw new Exception("Please provide a valid EntryId.");
                }
                else if(action == "create")
                {
                    return entryObj;
                }
            }
            else
            {
                return entryObj;
            }
            return entryObj;
        }
        catch(Exception ex)
        {
            throw new Exception("Exception: "+ ex);
        }
    }

    public static ObjModels.Property? CheckIfPropertyExists(string propertyId, CIRLibContext dbContext = null!, string action="")
    {
        try
        {
            if(string.IsNullOrWhiteSpace(propertyId))
            {
                throw new Exception("Please provide a valid PropertyId.");
            }
            var propertyObj = dbContext.Property.SingleOrDefault(item => item.PropertyId.Equals(propertyId));
            if(propertyObj == null)
            {
                if(action == "update")
                {
                    throw new Exception("Please provide a valid EntryId.");
                }
                else if(action == "create")
                {
                    return propertyObj;
                }
            }
            return propertyObj;
        }
        catch(Exception ex)
        {
            throw new Exception("Exception: "+ex);
        }
    }
}