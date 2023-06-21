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
        GetEquivalentEntriesByCIRIdAndEntryIdTest(dbContext);
        GetEquivalentEntriesByEntryIdTest(dbContext);
        GetEquivalentEntriesByCIRIdTest(dbContext);

        ModifyEntryDetailsTest(dbContext);

        AddPropertiesTest(dbContext);
        //ExceptionConditions
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
        var assertRegObj = dbContext.Registry.Where(item => item.RegistryId == "Assets");
        Assert.Single(assertRegObj);
        Assert.Equal("Assets",assertRegObj.First().RegistryId);

    }

    [Fact]
    public void AddCategoriesTest()
    {
        var dbContext = new MockContextFactory().GetDbContext();
        var newCatObj = new DataModel.CategoryDef()
        {
            CategoryId = "CatOfAssets",
            RegistryId = "Assets"
        };
        CIRManager.AddCategories(newCatObj, dbContext);
        var assertCatObj = dbContext.Category.Where(item => item.CategoryId == "CatOfAssets");
        Assert.Single(assertCatObj);
        Assert.Equal("CatOfAssets",assertCatObj.First().CategoryId);
    }
        
    [Fact]
    public void AddEntriesTestSingleObject()
    {   
        var dummyIdInSource = Guid.NewGuid().ToString();
        var newEntryObj = new DataModel.EntryDef()
            {
                IdInSource =  dummyIdInSource,
                SourceId = "NetworkCat",
                CategoryId = "CatOfBridges",
                RegistryId = "Bridge",
                CIRId = "CIRGroup1"
            };
        var dbContext = new MockContextFactory().GetDbContext();
        CIRManager.AddEntries(newEntryObj: newEntryObj, dbContext:dbContext);

        var AssertEntryObj = dbContext.Entry.Where(item => item.IdInSource.Equals(dummyIdInSource));
        Assert.Single(AssertEntryObj);
        Assert.Equal(dummyIdInSource, AssertEntryObj.First().IdInSource);
    }

    public void AddEntriesTest(CIRLibContext dbContext)
    {   
        var newEntryObj = new List<DataModel.EntryDef>()
        {
            new DataModel.EntryDef()
            {
                IdInSource = "Network",
                SourceId = "NetworkCat",
                CategoryId = "CatOfBridges",
                RegistryId = "Bridge",
                CIRId = "CIRGroup1"
            },
            new DataModel.EntryDef()
            {
                IdInSource = "Network201",
                SourceId = "NetworkCats",
                CategoryId = "CatOfBridges",
                RegistryId = "Bridge",
                CIRId = "CIRGroup1"
            }
        };
        CIRManager.AddEntries(newEntryObjs:newEntryObj, dbContext:dbContext);

        var AssertEntryObj = dbContext.Entry.Where(item => item.IdInSource.Equals("Network"));
        Assert.Single(AssertEntryObj);
        Assert.Equal("Network", AssertEntryObj.First().IdInSource);
    }
  
    public void GetEquivalentEntriesTest(CIRLibContext dbContext)
    {   
        var newEntryObj = new DataModel.EntryDef()
        {
            IdInSource = "Network",
            SourceId = "NetworkCat",
            CategoryId = "CatOfBridges",
            RegistryId = "Bridge"
        };
        var ListOfEntries = CIRManager.GetEquivalentEntries(newEntryObj, dbContext:dbContext);
        Assert.Equal("Bridge", ListOfEntries.First().RegistryId);
    }

    public void GetEquivalentEntriesByCIRIdAndEntryIdTest(CIRLibContext dbContext)
    {   
        var CIRIdSample = "CIRGroup1";
        var newEntryObj = new DataModel.EntryDef()
        {
            IdInSource = "Network",
            CIRId = CIRIdSample
        };
        var ListOfEntries = CIRManager.GetEquivalentEntries(newEntryObj, dbContext:dbContext);
        var count = dbContext.Entry.Where(a => a.CIRId == CIRIdSample).Count();
        Assert.Equal(count, ListOfEntries.Count());
    }

    public void GetEquivalentEntriesByEntryIdTest(CIRLibContext dbContext)
    {   
        var EntryIdSample = "Network201";
        var newEntryObj = new DataModel.EntryDef()
        {
            IdInSource = EntryIdSample
        };
        var ListOfEntries = CIRManager.GetEquivalentEntries(newEntryObj, dbContext:dbContext);
        Assert.Equal(EntryIdSample, ListOfEntries.First().IdInSource);
    }

    public void GetEquivalentEntriesByCIRIdTest(CIRLibContext dbContext)
    {   
        var CIRIdSample = "CIRGroup1";
        var newEntryObj = new DataModel.EntryDef()
        {
            CIRId = CIRIdSample
        };
        var ListOfEntries = CIRManager.GetEquivalentEntries(newEntryObj, dbContext:dbContext);
        var count = dbContext.Entry.Where(a => a.CIRId == CIRIdSample).Count();
        Assert.Equal(count, ListOfEntries.Count());
    }

    public void AddPropertiesTest(CIRLibContext dbContext)
    {
        var dummyKey = Guid.NewGuid().ToString();
        var newPropObj = new List<DataModel.PropertyDef>()
        {   new DataModel.PropertyDef
            {
                PropertyId = "NetworkProperty",
                EntryIdInSource = "Network", 
                Key = dummyKey,          
                Value = "NetworkValues",
                UnitOfMeasure = "Integer"
            },
            new DataModel.PropertyDef
            {
                PropertyId = "NetworkProperty201",
                EntryIdInSource = "Network",      
                Key = "NetworkKey201",      
                Value = "NetworkValues201",
                UnitOfMeasure = "Integer"
            }
        };

        CIRManager.AddProperties(newPropObjs:newPropObj, dbContext:dbContext);

        var AssertEntryObj = dbContext.Property.Where(item => item.PropertyId.Equals("NetworkProperty"));
        Assert.Single(AssertEntryObj);
        Assert.Equal("NetworkProperty", AssertEntryObj.First().PropertyId);

        var AssertObj = dbContext.Property.Where(item => item.PropertyId.Equals("NetworkProperty201"));
        Assert.Single(AssertObj);
        Assert.Equal("NetworkProperty201", AssertObj.First().PropertyId);

        var AssertEntryValueObj = dbContext.PropertyValue.Where(item => item.Value.Equals("NetworkValues"));
        Assert.Single(AssertEntryValueObj);
        Assert.Equal("NetworkValues", AssertEntryValueObj.First().Value);

        var AssertValueObj = dbContext.PropertyValue.Where(item => item.Value.Equals("NetworkValues201"));
        Assert.Single(AssertValueObj);
        Assert.Equal("NetworkValues201", AssertValueObj.First().Value);
    }

    public void AddPropertiesTestSingleObject(CIRLibContext dbContext)
    {
        var dummyKey = Guid.NewGuid().ToString();
        var dummyPropertyId = Guid.NewGuid().ToString("N");
        var dummyPropertyValues = Guid.NewGuid().ToString("N");
        var newPropObj =  new DataModel.PropertyDef
            {
                PropertyId = dummyPropertyId,
                EntryIdInSource = "Network", 
                Key = dummyKey,          
                Value = dummyPropertyValues,
                UnitOfMeasure = "Integer"
            };
        CIRManager.AddProperties(newPropObj:newPropObj, dbContext:dbContext);

        var AssertEntryObj = dbContext.Property.Where(item => item.PropertyId.Equals(dummyPropertyId));
        Assert.Single(AssertEntryObj);
        Assert.Equal(dummyPropertyId, AssertEntryObj.First().PropertyId);

        var AssertEntryValueObj = dbContext.PropertyValue.Where(item => item.Value.Equals(dummyPropertyValues));
        Assert.Single(AssertEntryValueObj);
        Assert.Equal(dummyPropertyValues, AssertEntryValueObj.First().Value);
    }

    public void AddPropertiesWithExceptionsTest(CIRLibContext dbContext)
    {
        var newPropObj = new List<DataModel.PropertyDef>()
        {   new DataModel.PropertyDef
            {
                //Not passing mandatory PropertyId here
                //BUT NetworkProperty301 should get created in the flow.
                EntryIdInSource = "Network", 
                Key = "NetworkKey",          
                Value = "NetworkValues",
                UnitOfMeasure = "Integer"
            },
            new DataModel.PropertyDef
            {
                PropertyId = "NetworkProperty301",
                EntryIdInSource = "Network",      
                Key = "NetworkKey301",      
                Value = "NetworkValues301",
                UnitOfMeasure = "Integer"
            }
        };

        Assert.Throws<System.ArgumentException>(() => CIRManager.AddProperties(newPropObjs:newPropObj, dbContext:dbContext));
        var AssertObj = dbContext.Property.Where(item => item.PropertyId.Equals("NetworkProperty301"));
        Assert.Single(AssertObj);
        Assert.Equal("NetworkProperty301", AssertObj.First().PropertyId);
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
                CategoryId = "CatOfBridges",
                RegistryId = "Bridge"
            },
            new DataModel.EntryDef()
            {
                IdInSource = $"Network401-{nameof(AddEntriesWithExceptionsTest)}",
                SourceId = "NetworkCat",
                CategoryId = "CatOfBridges",
                RegistryId = "Bridge"
            }
        };
        Assert.Throws<System.ArgumentException>(() => CIRManager.AddEntries(newEntryObjs:newEntryObj, dbContext:dbContext));

        var AssertEntryObj = dbContext.Entry
            .Where(item => item.IdInSource.Equals($"Network401-{nameof(AddEntriesWithExceptionsTest)}"));
        Assert.Single(AssertEntryObj);
        Assert.Equal($"Network401-{nameof(AddEntriesWithExceptionsTest)}", AssertEntryObj.First().IdInSource);
    }

    public void ModifyEntryDetailsTest(CIRLibContext dbContext)
    {
        var newEntryObj = new DataModel.EntryDef()
        {
            IdInSource = "Network",
            SourceId = "NetworkCat",
            CategoryId = "CatOfBridges",
            RegistryId = "Bridge",
            Name = "NetworkEntryName",
            Description = "NetworkEntryDescription"       
        };
        CIRManager.ModifyEntryDetails(newEntryObj,dbContext);
        var AssertEntryName = dbContext.Entry.Where(a => a.IdInSource.Equals(newEntryObj.IdInSource));
        Assert.Single(AssertEntryName);
        Assert.Equal("NetworkEntryName",AssertEntryName.First().Name);
        
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
            RegistryId = "Assets"
        };
        var newEntryObj = new List<DataModel.EntryDef>()
        {
            new DataModel.EntryDef()
            {
                IdInSource = "Network",
                SourceId = "NetworkCat",
                CategoryId = "CatOfBridges",
                RegistryId = "Bridge"
            }
        };
        var newPropObj = new List<DataModel.PropertyDef>()
        {
            new DataModel.PropertyDef()
            {
                PropertyId = "NetworkProperty",
                EntryIdInSource = "Network",            
                Value = "NetworkValues",
                UnitOfMeasure = "Integer"
            }
        };
        CIRManager.ProcessRegistry(newRegObj, newCatObj, newEntryObj, newPropObj, dbContext);

        var assertRegObj = dbContext.Registry.Where(item => item.RegistryId == "Assets");
        Assert.Single(assertRegObj);
        Assert.Equal("Assets",assertRegObj.First().RegistryId);

        var assertCatObj = dbContext.Category.Where(item => item.CategoryId == "CatOfAssets");
        Assert.Single(assertCatObj);
        Assert.Equal("CatOfAssets",assertCatObj.First().CategoryId);

        var AssertEntryObj = dbContext.Entry.Where(item => item.IdInSource.Equals("Network"));
        Assert.Single(AssertEntryObj);
        Assert.Equal("Network", AssertEntryObj.First().IdInSource);

        var AssertPropObj = dbContext.Property.Where(item => item.PropertyId.Equals("NetworkProperty"));
        Assert.Single(AssertPropObj);
        Assert.Equal("NetworkProperty", AssertPropObj.First().PropertyId);

        var AssertEntryValueObj = dbContext.PropertyValue.Where(item => item.Value.Equals("NetworkValues"));
        Assert.Single(AssertEntryValueObj);
        Assert.Equal("NetworkValues", AssertEntryValueObj.First().Value);
    }
}