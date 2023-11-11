using System.Collections.Immutable;
using System.Security.Claims;
using System.Text.Json;
using CIRLib.ObjectModel.Models;
using CIRServices;
using DataModelServices;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationExtesion.Support;

public static class RoleMappingExtensions
{
    public const string ROLE_MAPPINGS_REGISTRY = "__internal_role_mappings__";
    public const string ROLE_MAPPINGS_CATEGORY = "__internal_role_mappings__";
    private const string APPLICATION_ADMIN_ROLE = "FakeAdmin";
    private const string MAPPING_PROPERTY_ID = "UnidirectionalMappingID";

    public static void AddRoleMapping(Entry sourceRole, Entry newEquivalence, ClaimsPrincipal principal)
    {
        using var dbContext = CIRManager.Factory.CreateDbContext(principal).Result;

        CIRManager.AddProperties(new PropertyDef
        {
            PropertyId = MAPPING_PROPERTY_ID,
            EntryIdInSource = sourceRole.IdInSource,
            DataType = "JSON",
            Value = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "IDInSource", newEquivalence.IdInSource },
                { "SourceID", newEquivalence.SourceId }
            }, JsonSerializerOptions.Default),
        }, dbContext);
    }

    public static IEnumerable<Category> GetSystems(ClaimsPrincipal principal)
    {
        using var dbContext = CIRManager.Factory.CreateDbContext(principal).Result;
        return dbContext.Category
            .Include(c => c.Entries)
            .ThenInclude(e => e.Property)
            .ThenInclude(p => p.PropertyValues)
            .Where(c => c.CategoryId == ROLE_MAPPINGS_CATEGORY)
            .ToImmutableList();
    }

    public static IEnumerable<Entry> GetRoleMappings(Entry sourceRole, ClaimsPrincipal principal)
    {
        using var dbContext = CIRManager.Factory.CreateDbContext(principal).Result;
        sourceRole = dbContext.Entry
            .Where(e => e.Id == sourceRole.Id)
            .Include(e => e.Property)
            .ThenInclude(e => e.PropertyValues)
            .Single();
        // dbContext.Attach(sourceRole);
        // dbContext.Entry(sourceRole)
        //     .Collection(e => e.Property)
        //     .Query()
        //     .Where(p => p.PropertyId == MAPPING_PROPERTY_ID)
        //     .Include(p => p.PropertyValues)
        //     .Load();

        var mappingValues = sourceRole.Property.SingleOrDefault(p => p.PropertyId == MAPPING_PROPERTY_ID)?.PropertyValues
            .Select(pv => pv.ValueFromJson<Dictionary<string, string>>())
            .Where(pv => pv is not null)
            .Select(pv => (pv!["IDInSource"], pv["SourceID"]))
            .ToArray() ?? Array.Empty<(string, string)>();

        IQueryable<Entry>? query = null;
        foreach (var mapping in mappingValues)
        {
            var union = (
                from entry in dbContext.Entry
                where entry.IdInSource == mapping.Item1 && entry.SourceId == mapping.Item2 && entry.CategoryId == ROLE_MAPPINGS_CATEGORY
                select entry
            );
            query = query is null ? union : query.Concat(union);
        }

        return query?.AsNoTrackingWithIdentityResolution().ToImmutableArray() ?? Array.Empty<Entry>().ToImmutableArray();
    }

    public static void RemoveRoleMapping(Entry sourceRole, Entry deletedEquivalence, ClaimsPrincipal principal)
    {
        using var dbContext = CIRManager.Factory.CreateDbContext(principal).Result;

        var sourceEntry = dbContext.Entry
            .Where(e => e.Id == sourceRole.Id && 
                e.Property.Any(p => p.PropertyId == MAPPING_PROPERTY_ID &&
                    p.PropertyValues.Any(pv => pv.Value.Contains(deletedEquivalence.IdInSource))
                )
            )
            .Include(e => e.Property)
            .ThenInclude(p => p.PropertyValues)
            .FirstOrDefault();
        
        if (sourceEntry is null) return; // nothing to delete

        var valueMatch = new Dictionary<string, string>
        {
            { "IDInSource", deletedEquivalence.IdInSource },
            { "SourceID", deletedEquivalence.SourceId }
        };
        var propertyValue = sourceEntry.Property.Where(p => p.PropertyId == MAPPING_PROPERTY_ID)
            .SelectMany(p => p.PropertyValues)
            .Where(pv => !pv.ValueFromJson<Dictionary<string, string>>().Except(valueMatch).Any())
            .First();

        dbContext.Remove(propertyValue);
        dbContext.SaveChanges();

        // CIRManager.AddProperties(new PropertyDef
        // {
        //     PropertyId = MAPPING_PROPERTY_ID,
        //     EntryIdInSource = sourceRole.IdInSource,
        //     DataType = "JSON",
        //     Value = JsonSerializer.Serialize(new Dictionary<string, object>
        //     {
        //         { "IDInSource", newEquivalence.IdInSource },
        //         { "SourceID", newEquivalence.SourceId }
        //     }, JsonSerializerOptions.Default),
        // }, dbContext);
    }

    // Data seeding

    public static readonly (string SourceId, string Description)[] ROLE_MAPPING_CATEGORY_DETAILS = new[]
    {
        (SourceId: "Local Application", Description: "Internal application roles"),
        (SourceId: "Identity Provider", Description: "Roles/groups from the SSO identity provider (e.g., MS Active Directory)"),
        (SourceId: "Target Application", Description: "Roles/groups from the target or synched application (e.g., SAP)"),
    };

    public static void InitialiseRoleMappings(ClaimsPrincipal principal)
    {
        using var dbContext = CIRManager.Factory.CreateDbContext(principal).Result;

        var registry = dbContext.Registry.FirstOrDefault(r => r.RegistryId == ROLE_MAPPINGS_REGISTRY);
        registry ??= CIRManager.AddRegistries(new RegistryDef {
            RegistryId = ROLE_MAPPINGS_REGISTRY,
            Description = "Internal registry for tracking user role mappings."            
        }, dbContext);

        foreach (var (SourceId, Description) in ROLE_MAPPING_CATEGORY_DETAILS)
        {
            var category = dbContext.Category.FirstOrDefault(
                c => c.RegistryId == ROLE_MAPPINGS_REGISTRY && c.CategoryId == ROLE_MAPPINGS_CATEGORY && c.CategorySourceId == SourceId
            );
            category ??= CIRManager.AddCategories(new CategoryDef
            {
                CategoryId = ROLE_MAPPINGS_CATEGORY,
                SourceId = SourceId,
                RegistryId = ROLE_MAPPINGS_REGISTRY,
                Description = Description,
            }, dbContext);
        }

        var localCategory = dbContext.Category.Where(c => true).Include(c => c.Entries).First(c =>
            c.RegistryId == ROLE_MAPPINGS_REGISTRY
            && c.CategoryId == ROLE_MAPPINGS_CATEGORY
            && c.CategorySourceId == ROLE_MAPPING_CATEGORY_DETAILS[0].SourceId
        );

        var adminRole = localCategory.Entries.FirstOrDefault(e => e.IdInSource == APPLICATION_ADMIN_ROLE);
        adminRole ??= CIRManager.AddEntries(new EntryDef[] { new() {
            IdInSource = APPLICATION_ADMIN_ROLE,
            SourceId = localCategory.CategorySourceId, // Application ID? Get from config?
            SourceOwnerId = "Owning Organisation. From config?",
            RegistryId = ROLE_MAPPINGS_REGISTRY,
            CategoryId = ROLE_MAPPINGS_CATEGORY,
            CategorySourceId = localCategory.CategorySourceId,
            Name = "Admin",
            CIRId = Guid.NewGuid().ToString()
        } }, dbContext).First();

        var idpCategory = dbContext.Category.Where(c => true).Include(c => c.Entries).ThenInclude(e => e.Property).First(c =>
            c.RegistryId == ROLE_MAPPINGS_REGISTRY
            && c.CategoryId == ROLE_MAPPINGS_CATEGORY
            && c.CategorySourceId == ROLE_MAPPING_CATEGORY_DETAILS[1].SourceId
        );

        var idpRoles = new (string Id, string Name)[] {
            (Id: "RoadMapDevAdmin", "RoadMap Dev Administrators"),
            (Id: "RoadMapDevUsers", "RoadMap Dev Users"),
        };

        foreach (var (Id, Name) in idpRoles)
        {
            var idpRole = idpCategory.Entries.FirstOrDefault(e => e.IdInSource == Id);
            idpRole ??= CIRManager.AddEntries(new EntryDef[] { new()
            {
                IdInSource = Id,
                SourceId = idpCategory.CategorySourceId,
                SourceOwnerId = "Owning Organisation. From config?",
                RegistryId = ROLE_MAPPINGS_REGISTRY,
                CategoryId = ROLE_MAPPINGS_CATEGORY,
                CategorySourceId = idpCategory.CategorySourceId,
                Name = Name,
                CIRId = Guid.NewGuid().ToString(),
            } }, dbContext).First();

            if (!idpRole.Property.Any())
            {
                CIRManager.AddProperties(new PropertyDef
                {
                    PropertyId = MAPPING_PROPERTY_ID,
                    EntryIdInSource = idpRole.IdInSource,
                    DataType = "JSON",
                    Value = JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        { "IDInSource", adminRole.IdInSource },
                        { "SourceID", adminRole.SourceId }
                    }, JsonSerializerOptions.Default),
                }, dbContext);
            }
        }
    }
}