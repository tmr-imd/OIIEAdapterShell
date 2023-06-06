using DataModel = DataModelServices;
using CIRServices;
using CIRLib.Persistence;

namespace CIRLib.Test;
public class CIRManagerTest
{
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
    public void AddAndGetEquivalentEntriesTest()
    {
        //All Interface tests are sequentially called from this flow.
        AddEntriesTestWithNewRegistryCategoryAndEntryDetails();
    }
    
    public void AddEntriesTestWithNewRegistryCategoryAndEntryDetails()
    {   
        var dbContext = new MockContextFactory().GetDbContext();
        var newRegObj = new DataModel.RegistryDef() 
        {
            RegistryId = "Bridge"
        };
        var newCatObj = new DataModel.CategoryDef()
        {
            CategoryId = "CatOfBridges"
        };
        var newEntryObj = new DataModel.EntryDef()
        {
            IdInSource = "Network2",
            SourceId = "NetworkCat2"
        };

        CIRManager.AddEntries(newEntryObj, newRegObj, newCatObj, dbContext);
        
        var AssertEntryObj = dbContext.Entry.Where(item => item.IdInSource.Equals("Network2")).First();
        Assert.Equal("Network2", AssertEntryObj.IdInSource);

        AddEntriesTestWithEntryInfoOnly(dbContext);
    }
    public void AddEntriesTestWithEntryInfoOnly(CIRLibContext dbContext)
    {   
        var newEntryObj = new DataModel.EntryDef()
        {
            IdInSource = "Network",
            SourceId = "NetworkCat",
            CategoryRefId = "CatOfBridges",
            RegistryRefId = "Bridge"
        };
        
        CIRManager.AddEntries(newEntryObj:newEntryObj, dbContext:dbContext);

        var AssertEntryObj = dbContext.Entry.Where(item => item.IdInSource.Equals("Network")).First();
        Assert.Equal("Network", AssertEntryObj.IdInSource);
        
        //Testing the below with the above data.
        GetEquivalentEntriesTest(dbContext);
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

        //ModifyEntriesTest(dBContext);
    }


    //To do the below after updating the FK changes.
    // public void ModifyEntriesTest(CIRLibContext dBContext)
    // {
    //     var newEntryObj = new DataModel.EntryDef()
    //     {
    //         IdInSource = "Network",
    //         SourceId = "NetworkCat",
    //         CategoryRefId = "CatOfBridges",
    //         RegistryRefId = "Bridge",
    //         Description ="Updated"
    //     };
    //     CIRManager.ModifyEntryDetails(details, dBContext);

    //     var AssertEntryObj = dBContext.Entry.Where(item => item.IdInSource.Equals("Network1")).First();
    //     Assert.Equal("Updated", AssertEntryObj.Description);
    // }
}