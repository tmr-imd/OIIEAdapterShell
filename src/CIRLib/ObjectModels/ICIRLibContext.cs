using CIRLib.ObjectModel.Models;
using Microsoft.EntityFrameworkCore;

namespace CIRLib.ObjectModel
{
    public interface ICIRLibContext 
    {
        DbSet<Registry> Registry { get; set; }
        DbSet<Category> Category { get; set; }
        DbSet<Entry> Entry { get; set; }
        DbSet<Property> Property { get; set; }
        DbSet<PropertyValue> PropertyValue { get; set; }
    }
}
