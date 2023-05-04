
using Microsoft;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CIRLib;
using CIRLib.ObjectModel.Models;
using CIRLib.Persistence;
using CIRLib.Test;
using Xunit;

namespace CIRLib.Test;
public class CRUDModelOperationsTest
{   
    CIRLibContext mockDbContext = new MockContextFactory().getDbContext();
    [Fact]
    public void insertData()
    {   
        using (mockDbContext)
            {   
                var registryObj = new Registry { RegistryId = "Registration Server A", Description ="Registration Server A description"};
                var categoryObj = new Category { CategoryId = "Asset", SourceId = "MIMOSA OSA-EAI V3", Description = "MIMOSA OSA-EAI V3 description"  };
                var entryObj = new Entry { EntryId ="A101", SourceId ="EAM/CMMS System B", CIRId ="ISO/IEC 9834-8",
                                     IdInSource ="Oil Company A", Name ="A101", Description ="A101 desc", Inactive = false };
                var propertyObj = new Property { PropertyId="c", PropertyValue="PV101", DataType ="DT101"};
                var propertyValueObj = new PropertyValue { Key="PV101", Value="PV101", UnitOfMeasure ="Units"};

                mockDbContext.Registry.Add(registryObj);
                mockDbContext.Category.Add(categoryObj);
                mockDbContext.Entry.Add(entryObj);
                mockDbContext.Property.Add(propertyObj);
                mockDbContext.PropertyValue.Add(propertyValueObj);

                mockDbContext.SaveChanges();

                
                Assert.Equal(mockDbContext.Registry.First().RegistryId, registryObj.RegistryId);
                Assert.Equal(mockDbContext.Category.First().CategoryId, categoryObj.CategoryId);
                Assert.Equal(mockDbContext.Entry.First().EntryId, entryObj.EntryId);
                Assert.Equal(mockDbContext.Property.First().PropertyId, propertyObj.PropertyId);
                Assert.Equal(mockDbContext.PropertyValue.First().Key, propertyValueObj.Key);
                
                // var registryQuery = from item in mockDbContext.Registry select item;
                // var CategoryQuery = from item in mockDbContext.Category select item;
                // var EntryQuery = from item in mockDbContext.Entry select item;
                // var PropertyQuery = from item in mockDbContext.Property select item;
                // var PropertyValueQuery = from item in mockDbContext.PropertyValue select item;


                // Assert.Equal(registryQuery.First().RegistryId, registryObj.RegistryId);
                // Assert.Equal(CategoryQuery.First().CategoryId, categoryObj.CategoryId);
                // Assert.Equal(EntryQuery.First().EntryId, entryObj.EntryId);
                // Assert.Equal(PropertyQuery..PropertyId, propertyObj.PropertyId);
                // Assert.Equal(PropertyValueQuery.First().Key, propertyValueObj.Key);                

            }
        
    }

    [Fact]
    public void updateData()
    {   
        using (mockDbContext)
            { 
                //Registry_Table
                var regs = mockDbContext.Registry.Where(item => item.RegistryId.Contains("Registration Server A")).First();
                regs.Description = "Updated Registration Server A description";
                mockDbContext.SaveChanges();
                var updated_reg = mockDbContext.Registry.Where(item => item.RegistryId.Contains("Registration Server A")).First();
                Assert.Equal(updated_reg.Description,regs.Description);

                //Category_Table
                var cats = mockDbContext.Category.Where(item => item.CategoryId.Contains("Asset")).First();
                cats.Description = "Updated MIMOSA OSA-EAI V3 description";
                mockDbContext.SaveChanges();
                var updated_cats = mockDbContext.Category.Where(item => item.CategoryId.Contains("Asset")).First();
                Assert.Equal(updated_cats.Description,cats.Description);

                //Entry_Table
                var entries = mockDbContext.Entry.Where(item => item.EntryId.Contains("A101")).First();
                entries.Description = "Updated A101 desc";
                mockDbContext.SaveChanges();
                var updated_entries = mockDbContext.Entry.Where(item => item.EntryId.Contains("A101")).First();
                Assert.Equal(updated_entries.Description,entries.Description);

                //Property Table
                var props = mockDbContext.Property.Where(item => item.PropertyId.Contains("c")).First();
                props.PropertyValue = "Updated PV101";
                mockDbContext.SaveChanges();
                var updated_props = mockDbContext.Property.Where(item => item.PropertyId.Contains("c")).First();
                Assert.Equal(updated_props.PropertyValue,props.PropertyValue);

                //Property Value Table
                var prop_vals = mockDbContext.PropertyValue.Where(item => item.Key.Contains("PV101")).First();
                prop_vals.UnitOfMeasure = "Tonnes";
                mockDbContext.SaveChanges();
                var updated_prop_vals = mockDbContext.PropertyValue.Where(item => item.Key.Contains("PV101")).First();
                Assert.Equal(prop_vals.Key,updated_prop_vals.Key);

            }
        
    }

    [Fact]
    public void deleteData()
    {   
        using (mockDbContext)
            { 
                //Registry_Table
                var regs = mockDbContext.Registry.Where(item => item.RegistryId.Contains("Registration Server A")).First();
                mockDbContext.Registry.Remove(regs);                
                mockDbContext.SaveChanges();
                var updated_reg = mockDbContext.Registry.Where(item => item.RegistryId.Contains("Registration Server A"));
                Assert.Null(updated_reg);

                //Category_Table
                var cats = mockDbContext.Category.Where(item => item.CategoryId.Contains("Asset")).First();
                mockDbContext.Category.Remove(cats);                
                mockDbContext.SaveChanges();
                var updated_cats = mockDbContext.Category.Where(item => item.CategoryId.Contains("Asset"));
                Assert.Null(updated_cats);

                //Entry_Table
                var entries = mockDbContext.Entry.Where(item => item.EntryId.Contains("A101")).First();
                mockDbContext.Entry.Remove(entries);                
                mockDbContext.SaveChanges();
                var updated_entries = mockDbContext.Entry.Where(item => item.EntryId.Contains("A101"));
                Assert.Null(updated_entries);

                //Property Table
                var props = mockDbContext.Property.Where(item => item.PropertyId.Contains("c")).First();
                mockDbContext.Property.Remove(props);                
                mockDbContext.SaveChanges();
                var updated_props = mockDbContext.Property.Where(item => item.PropertyId.Contains("c"));
                Assert.Null(updated_props);

                //Property Value Table
                var prop_vals = mockDbContext.PropertyValue.Where(item => item.Key.Contains("PV101")).First();
                mockDbContext.PropertyValue.Remove(prop_vals);                
                mockDbContext.SaveChanges();
                var updated_prop_vals = mockDbContext.PropertyValue.Where(item => item.Key.Contains("PV101"));
                Assert.Null(updated_prop_vals);

            }
        
    }
    
}
