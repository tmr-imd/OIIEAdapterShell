using CIRLib.Persistence;
using System.Security.Claims;
using ObjModels = CIRLib.ObjectModel.Models;
using DataModel = DataModelServices;

namespace CIRServices;
public class CIRManager
{
    public static CIRLibContextFactory Factory {get; set;} = new CIRLibContextFactory();

    public static void AddRegistries(DataModel.RegistryDef newRegObj, CIRLibContext? dbContext = null)
    {
        if(string.IsNullOrWhiteSpace(newRegObj.RegistryId)){
            throw new ArgumentException("Mandatory field RegistryId not provided.");
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

    public static void AddCategories(DataModel.CategoryDef newCatObj, CIRLibContext? dbContext = null)
    {
        if(string.IsNullOrWhiteSpace(newCatObj.CategoryId)){
            throw new ArgumentException("Mandatory field CategoryId not provided.");
        }
        if(string.IsNullOrWhiteSpace(newCatObj.RegistryId))
        {
            throw new ArgumentException("Mandatory field RegistryId not provided.");
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

    public static void AddEntries(DataModel.EntryDef newEntryObj, CIRLibContext? dbContext = null)
    {
        AddEntries(new[] {newEntryObj}, dbContext);
    }
    public static void AddEntries(IEnumerable<DataModel.EntryDef> newEntryObjs, CIRLibContext? dbContext = null)
    {
        List<string> errorMessages = new List<string>();
            
        if (dbContext is null)
        {
            //This will always be null initially.
            //Added this to support dbContext passing for testing only.
            dbContext = Factory.CreateDbContext(new ClaimsPrincipal()).Result;
        }

        foreach(DataModel.EntryDef newEntryObj in newEntryObjs)
        {
            if(string.IsNullOrWhiteSpace(newEntryObj.IdInSource)
                || string.IsNullOrWhiteSpace(newEntryObj.CategoryId)
                || string.IsNullOrWhiteSpace(newEntryObj.RegistryId))
            {
                errorMessages.Add("Entry Id, CategoryId and RegistryId are mandatory fields!");
                errorMessages.Add("Mandatory fields not provided for: "+ newEntryObj);
                continue;
            }
            
            var eService = new EntryServices();       
            eService.CreateNewEntry(newEntryObj, dbContext);
        }
        if(errorMessages.Count != 0)
        {
            throw new ArgumentException(string.Join(" ",errorMessages));
        }
    }
    
    public static void AddProperties(DataModel.PropertyDef newPropObj, CIRLibContext? dbContext = null)
    {
        AddProperties(new[] {newPropObj}, dbContext);
    }
    public static void AddProperties(IEnumerable<DataModel.PropertyDef> newPropObjs, CIRLibContext? dbContext = null)
    {
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
            string.IsNullOrWhiteSpace(newPropObj.Value) || string.IsNullOrWhiteSpace(newPropObj.UnitOfMeasure))
            {
                errorMessages.Add("PropertyId, EntryIdInSource, Property Value, UnitOfMeasure are mandatory fields!");
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
                //Extracting PropertyModel and PropertyValueModel from Property DataModel.
                var propModelObj = new ObjModels.Property()
                {
                    PropertyId = newPropObj.PropertyId,
                    EntryIdInSource = newPropObj.EntryIdInSource,
                    PropertyValue = newPropObj.PropertyValue,
                    DataType = newPropObj.DataType
                };

                var pService = new PropertyServices();       
                pService.CreateNewProperty(newProperty: propModelObj, dbContext: dbContext);

                var propValueModelObj = new ObjModels.PropertyValue()
                {
                    Key = newPropObj.Key,
                    Value = newPropObj.Value,
                    UnitOfMeasure = newPropObj.UnitOfMeasure,
                    PropertyId = newPropObj.PropertyId,
                    Property =  propModelObj
                };

                var pvService = new PropertyValueServices();       
                pvService.CreateNewPropertyValue(propertyValueObj: propValueModelObj, propertyObj: propModelObj,
                    dbContext: dbContext);

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