using CIRLib.Persistence;
using System.Security.Claims;
using ObjModels = CIRLib.ObjectModel.Models;
namespace CIRServices;
public class CIRManager
{
    public static CIRLibContextFactory? Factory {get; set;}

    public static (ObjModels.Registry,ObjModels.Category,ObjModels.Entry) CreateObjectsFromProperties(object details)
    {
        var filterDetails = details.GetType().GetProperties();
        var newRegObj = new ObjModels.Registry();
        var newCatObj = new ObjModels.Category();
        var newEntryObj = new ObjModels.Entry();

        foreach (var property in filterDetails)
        {
            var propertyName = property.Name;
            var propertyValue = property.GetValue(details);
            var regPropertyInfo = newRegObj.GetType().GetProperty(propertyName);
            if (regPropertyInfo != null)
            {
                if(regPropertyInfo.CanWrite)
                {
                    regPropertyInfo.SetValue(newRegObj, propertyValue);
                }
            }
            else
            {
                var catPropertyInfo = newCatObj.GetType().GetProperty(propertyName);
                if (catPropertyInfo != null)
                {
                    if(catPropertyInfo.CanWrite)
                    {
                        catPropertyInfo.SetValue(newCatObj, propertyValue);
                    }
                }
                else
                {
                    var entryPropertyInfo = newEntryObj.GetType().GetProperty(propertyName);
                    if (entryPropertyInfo != null)
                    {
                        if(entryPropertyInfo.CanWrite)
                        {
                            entryPropertyInfo.SetValue(newEntryObj, propertyValue);
                        }
                    }
                }
            }
        }
        return (newRegObj, newCatObj, newEntryObj);
    }

    public static void checkMandatoryPropertiesPassed(ObjModels.Registry newRegObj, 
        ObjModels.Category newCatObj, ObjModels.Entry newEntryObj)
    {
        if(string.IsNullOrWhiteSpace(newEntryObj.IdInSource) || string.IsNullOrWhiteSpace(newEntryObj.SourceId))
        {
            throw new Exception("Mandatory Entry Identifiers not provided. ");
        }
    }

    public static List<ObjModels.Entry> GetEquivalentEntries(object details, CIRLibContext? dbContext = null)
    {
        var eService = new EntryServices();
        (ObjModels.Registry newRegObj, ObjModels.Category newCatObj, ObjModels.Entry newEntryObj) = 
            CreateObjectsFromProperties(details);
        checkMandatoryPropertiesPassed(newRegObj, newCatObj, newEntryObj);

        if (dbContext is null)
        {
            //This will always be null initially.
            //Added this to support dbContext passing for testing only.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }
        return eService.GetEntriesFromFilters(entryId: newEntryObj.IdInSource, entrySourceId: newEntryObj.SourceId,
         registryId: newRegObj.RegistryId, categoryId: newCatObj.CategoryId, categorySourceID: newCatObj.CategorySourceId,
         CIRId :newEntryObj.CIRId, dbContext: dbContext);        
    }

    public static void AddEntries(object details, CIRLibContext? dbContext = null)
    {
        var rService = new RegistryServices();
        var cService = new CategoryServices();
        var eService = new EntryServices();

        (ObjModels.Registry newRegObj, ObjModels.Category newCatObj, ObjModels.Entry newEntryObj) = 
            CreateObjectsFromProperties(details);

        if (dbContext is null)
        {
            //This will always be null initially.
            //Added this to support dbContext passing for testing only.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }

        if(!string.IsNullOrWhiteSpace(newRegObj.RegistryId))
        {
            rService.CreateNewRegistry(newRegObj, dbContext);
        }

        if(!string.IsNullOrWhiteSpace(newCatObj.CategoryId))
        {
            if(string.IsNullOrWhiteSpace(newCatObj.RegistryRefId))
            {
                // The RefId is a mandatoryfield to be passed.
                // Validation's should be covering this check.
                newCatObj.RegistryRefId = newRegObj.RegistryId;
            }
            cService.CreateNewCategory(newCatObj, dbContext);
        }
        if(!string.IsNullOrWhiteSpace(newEntryObj.IdInSource))
        {        
            if(string.IsNullOrWhiteSpace(newEntryObj.RegistryRefId))
            {
                // The RefId is a mandatoryfield to be passed.
                // Validation's should be covering this check.
                newEntryObj.RegistryRefId = newRegObj.RegistryId;
            }
            if(string.IsNullOrWhiteSpace(newEntryObj.CategoryRefId))
            {
                // The RefId is a mandatoryfield to be passed.
                // Validation's should be covering this check.
                newEntryObj.CategoryRefId = newCatObj.CategoryId;
            }
            eService.CreateNewEntry(newEntryObj, dbContext);
        }
    }
}