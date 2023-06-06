using CIRLib.Persistence;
using System.Security.Claims;
using ObjModels = CIRLib.ObjectModel.Models;
using DataModel = DataModelServices;

namespace CIRServices;
public class CIRManager
{
    public static CIRLibContextFactory? Factory {get; set;}

    public static void AddRegistries(DataModel.RegistryDef newRegObj, CIRLibContext dbContext = null)
    {
        if(string.IsNullOrWhiteSpace(newRegObj.RegistryId)){
            throw new Exception("Mandatory field RegistryId not provided.");
        }
        if (dbContext is null)
        {
            //dbContext will always be assigned here.
            //For Testing, dbContext will be passed from the Test function.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }

        var rService = new RegistryServices();
        rService.CreateNewRegistry(newRegObj, dbContext);
    }

    public static void AddCategories(DataModel.CategoryDef newCatObj, CIRLibContext dbContext)
    {
        if(string.IsNullOrWhiteSpace(newCatObj.CategoryId)){
            throw new Exception("Mandatory field CategoryId not provided.");
        }
        if(string.IsNullOrWhiteSpace(newCatObj.RegistryRefId))
        {
            throw new Exception("Mandatory field RegistryRefId not provided.");
        }
        
        if (dbContext is null)
        {
            //dbContext will always be assigned here.
            //For Testing, dbContext will be passed from the Test function.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }

        var cService = new CategoryServices();
        cService.CreateNewCategory(newCatObj, dbContext);
    }
    public static List<ObjModels.Entry> GetEquivalentEntries( DataModel.EntryDef newEntryObj,
        CIRLibContext? dbContext = null)
    {
        var eService = new EntryServices();
        CommonServices.checkMandatoryPropertiesPassed(newEntryObj);
        if (dbContext is null)
        {
            //This will always be null initially.
            //Added this to support dbContext passing for testing only.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }
        return eService.GetEntriesFromFilters(entryId: newEntryObj.IdInSource, entrySourceId: newEntryObj.SourceId,
         registryId: newEntryObj.RegistryRefId, categoryId: newEntryObj.CategoryRefId,
         CIRId :newEntryObj.CIRId, dbContext: dbContext);        
    }

    public static void AddEntries(DataModel.EntryDef newEntryObj, DataModel.RegistryDef? newRegObj=null, 
        DataModel.CategoryDef? newCatObj=null, CIRLibContext? dbContext = null)
    {
        CommonServices.checkMandatoryPropertiesPassed(newEntryObj, newRegObj, newCatObj);
        var rService = new RegistryServices();
        var cService = new CategoryServices();
        var eService = new EntryServices();
            
        if (dbContext is null)
        {
            //This will always be null initially.
            //Added this to support dbContext passing for testing only.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }
        
        if((newRegObj!=null) &&  !string.IsNullOrWhiteSpace(newRegObj.RegistryId))
        {
            rService.CreateNewRegistry(newRegObj, dbContext);
        }

        if((newCatObj!=null) && !string.IsNullOrWhiteSpace(newCatObj.CategoryId))
        {
            if(string.IsNullOrWhiteSpace(newCatObj.RegistryRefId) && newRegObj!=null)
            {
                newCatObj.RegistryRefId = newRegObj.RegistryId;
            }
            else
            {
                throw new Exception("Mandatory RegistryRefId for Category was not provided. ");
            }
            cService.CreateNewCategory(newCatObj, dbContext);
        }

        if((newEntryObj!=null) && !string.IsNullOrWhiteSpace(newEntryObj.IdInSource))
        {        
            if(string.IsNullOrWhiteSpace(newEntryObj.RegistryRefId))
            {
                if(!string.IsNullOrWhiteSpace(newRegObj.RegistryId))
                {
                    newEntryObj.RegistryRefId = newRegObj.RegistryId;
                }
                else if(!string.IsNullOrWhiteSpace(newCatObj.RegistryRefId))
                {
                    newEntryObj.RegistryRefId = newCatObj.RegistryRefId;
                }
                else
                {
                    throw new Exception("Required field RegistryRefId is not provided.");
                }
            }

            if(string.IsNullOrWhiteSpace(newEntryObj.CategoryRefId))
            {
                if(!string.IsNullOrWhiteSpace(newCatObj.CategoryId))
                {
                    newEntryObj.CategoryRefId = newCatObj.CategoryId;
                }
                else
                {
                    throw new Exception("Required field RegistryRefId is not provided.");
                }
            }
            eService.CreateNewEntry(newEntryObj, dbContext);
        }
    }
    

    // To Do: the below after updating the FK changes needed.

    // public static void ModifyEntryDetails(DataModel.EntryDef newEntryObj, DataModel.RegistryDef? newRegObj, 
    //     DataModel.CategoryDef? newCatObj, CIRLibContext? dbContext = null)
    // {
    //     checkMandatoryPropertiesPassed(newEntryObj);
    //     var eService = new EntryServices();
    //     eService.UpdateEntry(newEntryObj.Id, newEntryObj, dbContext);
    // }
}