using CIRLib.Persistence;
using System.Security.Claims;
using ObjModels = CIRLib.ObjectModel.Models;
using DataModel = DataModelServices;
using Microsoft.Extensions.Logging;

namespace CIRServices;
public class CIRManager
{
    public static CIRLibContextFactory Factory {get; set;} = new CIRLibContextFactory();
    private static ILogger<CIRManager> _logger;

    public CIRManager(ILogger<CIRManager> logger)
    {
        _logger = logger;
    }

    public static ObjModels.Registry AddRegistries(DataModel.RegistryDef newRegObj, CIRLibContext? dbContext = null)
    {
        if(string.IsNullOrWhiteSpace(newRegObj.RegistryId)){
            throw new ArgumentException("Mandatory field RegistryId not provided.");
        }
        //dbContext will always be assigned here.
        //For Testing, dbContext will be passed from the Test function.
        dbContext ??= Factory.CreateDbContext(new ClaimsPrincipal()).Result;

        var rService = new RegistryServices();
        return rService.CreateNewRegistry(newRegObj, dbContext);
    }

    public static ObjModels.Category AddCategories(DataModel.CategoryDef newCatObj, CIRLibContext? dbContext = null)
    {
        if(string.IsNullOrWhiteSpace(newCatObj.CategoryId)){
            throw new ArgumentException("Mandatory field CategoryId not provided.");
        }
        if(string.IsNullOrWhiteSpace(newCatObj.RegistryId))
        {
            throw new ArgumentException("Mandatory field RegistryId not provided.");
        }

        //dbContext will always be assigned here.
        //For Testing, dbContext will be passed from the Test function.
        dbContext ??= Factory.CreateDbContext(new ClaimsPrincipal()).Result;

        var cService = new CategoryServices();
        return cService.CreateNewCategory(newCatObj, dbContext);
    }


    public static IEnumerable<ObjModels.Entry> GetEquivalentEntries( DataModel.EntryDef newEntryObj,
        CIRLibContext? dbContext = null)
    {
        if (dbContext is null)
        {
            //This will always be null initially.
            //Added this to support dbContext passing for testing only.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }

        return new EntryServices().GetEntriesFromFilters(entryId: newEntryObj.IdInSource, entrySourceId: newEntryObj.SourceId,
         registryId: newEntryObj.RegistryId, categoryId: newEntryObj.CategoryId,
         CIRId :newEntryObj.CIRId, dbContext: dbContext);        
    }

    public static IEnumerable<ObjModels.Entry> GetEquivalentEntries(string entryId = "", string entrySourceId = "", string registryId = "",
        string categoryId = "", string categorySourceID = "", string propertyId = "",
        string propertyValueKey = "", string CIRId = "", CIRLibContext? dbContext = null)
    {
        if (dbContext is null)
        {
            //This will always be null initially.
            //Added this to support dbContext passing for testing only.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }

        return new EntryServices().GetEntriesFromFilters(entryId, entrySourceId, registryId, categoryId, categorySourceID, propertyId ,
        propertyValueKey, CIRId, dbContext: dbContext);
    }

    public static IEnumerable<ObjModels.Entry> AddEntries(IEnumerable<DataModel.EntryDef> newEntryObjs, CIRLibContext? dbContext = null)
    {
        List<string> errorMessages = new();

        //This will always be null initially.
        //Added this to support dbContext passing for testing only.
        dbContext ??= Factory.CreateDbContext(new ClaimsPrincipal()).Result;

        var results = newEntryObjs.Select(newEntryObj => {
            if(string.IsNullOrWhiteSpace(newEntryObj.IdInSource)
                || string.IsNullOrWhiteSpace(newEntryObj.CategoryId)
                || string.IsNullOrWhiteSpace(newEntryObj.CategorySourceId)
                || string.IsNullOrWhiteSpace(newEntryObj.RegistryId))
            {
                _logger.LogInformation("Creating Placeholder Category and Registry as the Category and Registry details were not present.");
            }
            
            var eService = new EntryServices();
            return eService.CreateNewEntry(newEntryObj, dbContext);
        });

        if (errorMessages.Count != 0)
        {
            throw new ArgumentException(string.Join(" ", errorMessages));
        }

        return results;
    }
    
    public static void AddProperties(DataModel.PropertyDef newPropObj, CIRLibContext? dbContext = null)
    {
        AddProperties(new[] {newPropObj}, dbContext);
    }
    public static void AddProperties(IEnumerable<DataModel.PropertyDef> newPropObjs, CIRLibContext? dbContext = null)
    {
        var pService = new PropertyServices();
        var pvService = new PropertyValueServices();
        List<string> errorMessages = new List<string>();

        if (dbContext is null)
        {
            //dbContext will always be assigned here.
            //For Testing, dbContext will be passed from the Test function.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }

        foreach(DataModel.PropertyDef newPropObj in newPropObjs)
        {
            if(string.IsNullOrWhiteSpace(newPropObj.PropertyId) ||
            string.IsNullOrWhiteSpace(newPropObj.EntryIdInSource) ||
            string.IsNullOrWhiteSpace(newPropObj.Value))
            {
                errorMessages.Add("PropertyId, EntryIdInSource, Property Value are mandatory fields!");
                errorMessages.Add("Mandatory fields not provided for: "+ newPropObj);
                continue;
            }

            var entryObj = dbContext.Entry.FirstOrDefault(item => item.IdInSource.Contains(newPropObj.EntryIdInSource));

            if( entryObj == null)
            {
                errorMessages.Add("Enter a valid EntryIdInSource. ");
                continue;
            }
            else
            {

                var property = pService.FindOrCreateNewProperty(newProperty: newPropObj, dbContext: dbContext);
                
                if (!property.PropertyValues.Any(pv => pv.Key == newPropObj.Key && pv.UnitOfMeasure == newPropObj.UnitOfMeasure && pv.Value == newPropObj.Value))
                {
                    pvService.CreateNewPropertyValue(propertyValueObj: newPropObj, propertyObj: property,
                        dbContext: dbContext);
                }
            }
        }

        if(errorMessages.Count != 0)
        {
            throw new ArgumentException(string.Join(" ",errorMessages));
        }
    }

    public static void ModifyEntryDetails(DataModel.EntryDef newEntryObj, CIRLibContext? dbContext = null)
    {
        if (dbContext is null)
        {
            //This will always be null initially.
            //Added this to support dbContext passing for testing only.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }
        
        var entryExists = CommonServices.CheckIfEntryExists(newEntryObj.IdInSource, dbContext);
        if(entryExists != null)
        {
            var eService = new EntryServices();
            eService.UpdateEntry(entryExists.Id, newEntryObj, dbContext);
        }
    }

    public static void UpdateCIRIdInEntryDetails(string CIRId, ObjModels.Entry newEntryObj, CIRLibContext? dbContext = null)
    {
        if (dbContext is null)
        {
            //This will always be null initially.
            //Added this to support dbContext passing for testing only.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }

        var eService = new EntryServices();
        eService.UpdateCIRIdInEntry(CIRId, newEntryObj, dbContext);
    }

    /// <summary>
    /// Retrieves the CIRId from the existing Entry.
    /// </summary>
    /// <param name="entryObj">Entry Object which will contain all the fields to filter for the corresponding CIRId</param>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public static string GetCIRIdFromEntry(ObjModels.Entry entryObj, CIRLibContext? dbContext = null)
    {
        if (dbContext is null)
        {
            //This will always be null initially.
            //Added this to support dbContext passing for testing only.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }

        var eService = new EntryServices();
        var entry = eService.GetEntriesFromFilters(entryId: entryObj.IdInSource, entrySourceId: entryObj.SourceId,
         registryId: entryObj.RegistryId, categoryId: entryObj.CategoryId, dbContext: dbContext).FirstOrDefault();
        return entry.CIRId;
    }

    /// <summary>
    /// Get's the IdInSource from the existing Entries.
    /// Expecting to get unique values and hence a FirstorDefault() is added to the Filter.
    /// </summary>
    /// <param name="entryObj">Entry Object which will have all the fields necessary to retrieve the corresponding IdInSource</param>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public static string GetIdInSourceFromEntry(ObjModels.Entry entryObj, CIRLibContext? dbContext = null)
    {
        if (dbContext is null)
        {
            //This will always be null initially.
            //Added this to support dbContext passing for testing only.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }

        var eService = new EntryServices();
        var entry = eService.GetEntriesFromFilters(entrySourceId: entryObj.SourceId,
         registryId: entryObj.RegistryId, categoryId: entryObj.CategoryId, CIRId: entryObj.CIRId, dbContext: dbContext).FirstOrDefault();
        return entry.IdInSource;
    }

    public static void ProcessRegistry(DataModel.RegistryDef regObj, DataModel.CategoryDef catObj,
        IEnumerable<DataModel.EntryDef> entObj, IEnumerable<DataModel.PropertyDef> propObj,
        CIRLibContext? dbContext = null)
    {
        if (dbContext is null)
        {
            //This will always be null initially.
            //Added this to support dbContext passing for testing only.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }
        AddRegistries(regObj,dbContext);
        AddCategories(catObj,dbContext);
        AddEntries(entObj,dbContext);
        AddProperties(propObj,dbContext);
    }
}