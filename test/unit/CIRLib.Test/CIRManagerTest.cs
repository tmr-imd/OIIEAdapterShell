using DataModel = DataModelServices;
using CIRServices;
using CIRLib.Persistence;

namespace CIRLib.Test;
public class CIRManagerTest
{
    [Fact]
    public void AddAndGetEquivalentEntriesTest()
    {
        var dbContext = new MockContextFactory().GetDbContext();
        AddEntriesTest(dbContext);
        GetEquivalentEntriesTest(dbContext);
        ModifyEntryDetailsTest(dbContext);
        AddPropertiesTest(dbContext);
        AddPropertiesWithExceptionsTest(dbContext);
        AddEntriesWithExceptionsTest(dbContext);
        AddPropertiesTestSingleObject(dbContext);
    }
    
    [Fact]
    public void AddRegistriesTest()
    {
        var dbContext = new MockContextFactory().GetDbContext();
        var newRegObj = new DataModel.RegistryDef() 
        {
            RegistryId = "Assets"
        };
        CIRManager.AddRegistries(newRegObj,dbContext);
        var assertRegObj = dbContext.Registry.Where(item => item.RegistryId == "Assets").First();
        Assert.Equal("Assets",assertRegObj.RegistryId);

    }

    [Fact]
    public void AddCategoriesTest()
    {
        var dbContext = new MockContextFactory().GetDbContext();
        var newCatObj = new DataModel.CategoryDef()
        {
            CategoryId = "CatOfAssets",
            RegistryRefId = "Assets"
        };
        CIRManager.AddCategories(newCatObj, dbContext);
        var assertRegObj = dbContext.Category.Where(item => item.CategoryId == "CatOfAssets").First();
        Assert.Equal("CatOfAssets",assertRegObj.CategoryId);

    }
        
    [Fact]
    public void AddEntriesTestSingleObject()
    {   
        var dummyIdInSource = Guid.NewGuid().ToString();
        var newEntryObj = new DataModel.EntryDef()
            {
                IdInSource =  dummyIdInSource,
                SourceId = "NetworkCat",
                CategoryRefId = "CatOfBridges",
                RegistryRefId = "Bridge"
            };
        var dbContext = new MockContextFactory().GetDbContext();
        CIRManager.AddEntries(newEntryObj: newEntryObj, dbContext:dbContext);

        var AssertEntryObj = dbContext.Entry.Where(item => item.IdInSource.Equals(dummyIdInSource)).First();
        Assert.Equal(dummyIdInSource, AssertEntryObj.IdInSource);
    }

    public void AddEntriesTest(CIRLibContext dbContext)
    {   
        var newEntryObj = new List<DataModel.EntryDef>()
        {
            new DataModel.EntryDef()
            {
                IdInSource = "Network",
                SourceId = "NetworkCat",
                CategoryRefId = "CatOfBridges",
                RegistryRefId = "Bridge"
            },
            new DataModel.EntryDef()
            {
                IdInSource = "Network201",
                SourceId = "NetworkCats",
                CategoryRefId = "CatOfBridges",
                RegistryRefId = "Bridge"
            }
        };
        CIRManager.AddEntries(newEntryObjs:newEntryObj, dbContext:dbContext);

        var AssertEntryObj = dbContext.Entry.Where(item => item.IdInSource.Equals("Network")).First();
        Assert.Equal("Network", AssertEntryObj.IdInSource);
    }
  
    public void GetEquivalentEntriesTest(CIRLibContext dbContext)
    {   
        var newEntryObj = new DataModel.EntryDef()
        {
            IdInSource = "Network",
            SourceId = "NetworkCat",
            CategoryRefId = "CatOfBridges",
            RegistryRefId = "Bridge"
        };
        var ListOfEntries = CIRManager.GetEquivalentEntries(newEntryObj, dbContext:dbContext);
        
        Assert.Equal("Bridge", ListOfEntries.First().RegistryRefId);
    }

    public void AddPropertiesTest(CIRLibContext dbContext)
    {
        var dummyKey = Guid.NewGuid().ToString();
        var newPropObj = new List<DataModel.PropertyDef>()
        {   new DataModel.PropertyDef
            {
                PropertyId = "NetworkProperty",
                EntryRefIdInSource = "Network", 
                Key = dummyKey,          
                Value = "NetworkValues",
                UnitOfMeasure = "Integer"
            },
            new DataModel.PropertyDef
            {
                PropertyId = "NetworkProperty201",
                EntryRefIdInSource = "Network",      
                Key = "NetworkKey201",      
                Value = "NetworkValues201",
                UnitOfMeasure = "Integer"
            }
        };

        CIRManager.AddProperties(newPropObjs:newPropObj, dbContext:dbContext);

        var AssertEntryObj = dbContext.Property.Where(item => item.PropertyId.Equals("NetworkProperty")).First();
        Assert.Equal("NetworkProperty", AssertEntryObj.PropertyId);

        var AssertObj = dbContext.Property.Where(item => item.PropertyId.Equals("NetworkProperty201")).First();
        Assert.Equal("NetworkProperty201", AssertObj.PropertyId);

        var AssertEntryValueObj = dbContext.PropertyValue.Where(item => item.Value.Equals("NetworkValues")).First();
        Assert.Equal("NetworkValues", AssertEntryValueObj.Value);
        var AssertValueObj = dbContext.PropertyValue.Where(item => item.Value.Equals("NetworkValues201")).First();
        Assert.Equal("NetworkValues201", AssertValueObj.Value);
    }

    public void AddPropertiesTestSingleObject(CIRLibContext dbContext)
    {
        var dummyKey = Guid.NewGuid().ToString();
        var dummyPropertyId = Guid.NewGuid().ToString("N");
        var dummyPropertyValues = Guid.NewGuid().ToString("N");
        var newPropObj =  new DataModel.PropertyDef
            {
                PropertyId = dummyPropertyId,
                EntryRefIdInSource = "Network", 
                Key = dummyKey,          
                Value = dummyPropertyValues,
                UnitOfMeasure = "Integer"
            };
        CIRManager.AddProperties(newPropObj:newPropObj, dbContext:dbContext);

        var AssertEntryObj = dbContext.Property.Where(item => item.PropertyId.Equals(dummyPropertyId)).First();
        Assert.Equal(dummyPropertyId, AssertEntryObj.PropertyId);

        var AssertEntryValueObj = dbContext.PropertyValue.Where(item => item.Value.Equals(dummyPropertyValues)).First();
        Assert.Equal(dummyPropertyValues, AssertEntryValueObj.Value);
    }

    public void AddPropertiesWithExceptionsTest(CIRLibContext dbContext)
    {
        var newPropObj = new List<DataModel.PropertyDef>()
        {   new DataModel.PropertyDef
            {
                //Not passing mandatory PropertyId here
                //BUT NetworkProperty301 should get created in the flow.
                EntryRefIdInSource = "Network", 
                Key = "NetworkKey",          
                Value = "NetworkValues",
                UnitOfMeasure = "Integer"
            },
            new DataModel.PropertyDef
            {
                PropertyId = "NetworkProperty301",
                EntryRefIdInSource = "Network",      
                Key = "NetworkKey301",      
                Value = "NetworkValues301",
                UnitOfMeasure = "Integer"
            }
        };

        Assert.Throws<System.Exception>(() => CIRManager.AddProperties(newPropObjs:newPropObj, dbContext:dbContext));
        var AssertObj = dbContext.Property.Where(item => item.PropertyId.Equals("NetworkProperty301")).First();
        Assert.Equal("NetworkProperty301", AssertObj.PropertyId);
    }

    public void AddEntriesWithExceptionsTest(CIRLibContext dbContext)
    {   
        var newEntryObj = new List<DataModel.EntryDef>()
        {
            new DataModel.EntryDef()
            {
                //IdInSource = "Network",
                //Removing the above should throw an error but continue with create the below entry.
                SourceId = "NetworkCat",
                CategoryRefId = "CatOfBridges",
                RegistryRefId = "Bridge"
            },
            new DataModel.EntryDef()
            {
                IdInSource = $"Network401-{nameof(AddEntriesWithExceptionsTest)}",
                SourceId = "NetworkCat",
                CategoryRefId = "CatOfBridges",
                RegistryRefId = "Bridge"
            }
        };
        Assert.Throws<System.Exception>(() => CIRManager.AddEntries(newEntryObjs:newEntryObj, dbContext:dbContext));

        var AssertEntryObj = dbContext.Entry
            .Where(item => item.IdInSource.Equals($"Network401-{nameof(AddEntriesWithExceptionsTest)}")).First();
        Assert.Equal($"Network401-{nameof(AddEntriesWithExceptionsTest)}", AssertEntryObj.IdInSource);
    }

    public void ModifyEntryDetailsTest(CIRLibContext dbContext)
    {
        var newEntryObj = new DataModel.EntryDef()
        {
            IdInSource = "Network",
            SourceId = "NetworkCat",
            CategoryRefId = "CatOfBridges",
            RegistryRefId = "Bridge",
            Name = "NetworkEntryName",
            Description = "NetworkEntryDescription"       
        };
        CIRManager.ModifyEntryDetails(newEntryObj,dbContext);
        var AssertEntryName = dbContext.Entry.Where(a => a.IdInSource.Equals(newEntryObj.IdInSource)).First();
        Assert.Equal("NetworkEntryName",AssertEntryName.Name);
        
    }

    [Fact]
    public void ProcessRegistryTest()
    {
        var dbContext = new MockContextFactory().GetDbContext();
        var newRegObj = new DataModel.RegistryDef() 
        {
            RegistryId = "Assets"
        };
        var newCatObj = new DataModel.CategoryDef()
        {
            CategoryId = "CatOfAssets",
            RegistryRefId = "Assets"
        };
        var newEntryObj = new List<DataModel.EntryDef>()
        {
            new DataModel.EntryDef()
            {
                IdInSource = "Network",
                SourceId = "NetworkCat",
                CategoryRefId = "CatOfBridges",
                RegistryRefId = "Bridge"
            }
        };
        var newPropObj = new List<DataModel.PropertyDef>()
        {
            new DataModel.PropertyDef()
            {
                PropertyId = "NetworkProperty",
                EntryRefIdInSource = "Network",            
                Value = "NetworkValues",
                UnitOfMeasure = "Integer"
            }
        };
        CIRManager.ProcessRegistry(newRegObj, newCatObj, newEntryObj, newPropObj, dbContext);

        var assertRegObj = dbContext.Registry.Where(item => item.RegistryId == "Assets").First();
        Assert.Equal("Assets",assertRegObj.RegistryId);

        var assertCatObj = dbContext.Category.Where(item => item.CategoryId == "CatOfAssets").First();
        Assert.Equal("CatOfAssets",assertCatObj.CategoryId);

        var AssertEntryObj = dbContext.Entry.Where(item => item.IdInSource.Equals("Network")).First();
        Assert.Equal("Network", AssertEntryObj.IdInSource);

        var AssertPropObj = dbContext.Property.Where(item => item.PropertyId.Equals("NetworkProperty")).First();
        Assert.Equal("NetworkProperty", AssertPropObj.PropertyId);

        var AssertEntryValueObj = dbContext.PropertyValue.Where(item => item.Value.Equals("NetworkValues")).First();
        Assert.Equal("NetworkValues", AssertEntryValueObj.Value);
    }
}