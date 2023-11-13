using System.Collections.Immutable;
using System.Security.Claims;
using System.Text.Json;
using CIRLib.ObjectModel.Models;
using CIRLib.Persistence;
using CIRServices;
using DataModelServices;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthenticationExtesion.Support;

public class RoleMappingService : IDisposable, IAsyncDisposable
{
    public const string ROLE_MAPPINGS_REGISTRY = "__internal_role_mappings__";
    public const string ROLE_MAPPINGS_CATEGORY = "__internal_role_mappings__";
    private const string APPLICATION_ADMIN_ROLE = "FakeAdmin";
    private const string MAPPING_PROPERTY_ID = "UnidirectionalMappingID";

    private readonly ILogger<RoleMappingService> _logger;
    private readonly CIRLibContextFactory _factory;
    private readonly UserService _userService;
    private ClaimsPrincipal _user;
    private CIRLibContext _dbContext;

    // TODO: check for change in user and recreate the context

    public RoleMappingService(UserService userService,
            CIRLibContextFactory factory, ILogger<RoleMappingService> logger)
    {
        _userService = userService;
        _user = _userService.CurrentUser;
        _logger = logger;
        _factory = factory;
        _dbContext = _factory.CreateDbContext(_user).Result;
    }

    public Entry AddRole(EntryDef newRole)
    {
        CheckUserChanged();

        return CIRManager.AddEntries(new EntryDef[] { newRole }, _dbContext).Single();
    }

    public void AddRoleMapping(Entry sourceRole, Entry newEquivalence)
    {
        CheckUserChanged();

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
        }, _dbContext);
    }

    public IEnumerable<Category> GetSystems(IEnumerable<Guid>? ids = null)
    {
        CheckUserChanged();

        var query = _dbContext.Category
            .Include(c => c.Entries)
            .ThenInclude(e => e.Property)
            .ThenInclude(p => p.PropertyValues)
            .Where(c => c.CategoryId == ROLE_MAPPINGS_CATEGORY);

        if (ids is {}) query = query.Where(c => ids.Contains(c.Id));

        return query.ToImmutableList();
    }

    public Entry? GetRole(Guid id)
    {
        CheckUserChanged();
        return _dbContext.Entry.Find(id);
    }

    public IEnumerable<Entry> GetRoleMappings(Entry sourceRole)
    {
        CheckUserChanged();
        
        try
        {
            sourceRole = _dbContext.Entry
                .Where(e => e.Id == sourceRole.Id)
                .Include(e => e.Property)
                .ThenInclude(e => e.PropertyValues)
                .Single();

            var mappingValues = sourceRole.Property.SingleOrDefault(p => p.PropertyId == MAPPING_PROPERTY_ID)?.PropertyValues
                .Select(pv => pv.ValueFromJson<Dictionary<string, string>>())
                .Where(pv => pv is not null)
                .Select(pv => (pv!["IDInSource"], pv["SourceID"]))
                .ToArray() ?? Array.Empty<(string, string)>();

            IQueryable<Entry>? query = null;
            foreach (var (idInSource, sourceId) in mappingValues)
            {
                var union = (
                    from entry in _dbContext.Entry
                    where entry.IdInSource == idInSource && entry.SourceId == sourceId && entry.CategoryId == ROLE_MAPPINGS_CATEGORY
                    select entry
                );
                query = query is null ? union : query.Concat(union);
            }

            _logger.LogTrace("SQL> {MappedRoles}", _logger.IsEnabled(LogLevel.Trace) ? query?.ToQueryString() : "");
            return query?.AsNoTrackingWithIdentityResolution().ToImmutableArray() ?? Array.Empty<Entry>().ToImmutableArray();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogDebug(ex, "No role for {SourceRole} (likely was deleted)", sourceRole.IdInSource);
            return Array.Empty<Entry>();
        }
    }

    public void RemoveRole(Guid roleId)
    {
        CheckUserChanged();
        
        var role = _dbContext.Entry.Find(roleId);
        if (role is null) return; // already removed
        
        _dbContext.Remove(role);
        
        // Remove all mappings to the role being removed.
        PropertyValue prop = new ();
        prop.ValueFromJson(new Dictionary<string, string>
        {
            { "IDInSource", role.IdInSource },
            { "SourceID", role.SourceId }
        });
        _dbContext.PropertyValue
            .Where(pv => pv.Property.PropertyId == MAPPING_PROPERTY_ID && pv.Value == prop.Value)
            .Select(pv => _dbContext.Remove(pv))
            .Load();

        _dbContext.SaveChanges();
    }

    public void RemoveRoleMapping(Entry sourceRole, Entry deletedEquivalence)
    {
        CheckUserChanged();

        var sourceEntry = _dbContext.Entry
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
            .Where(pv => !pv.ValueFromJson<Dictionary<string, string>>()?.Except(valueMatch).Any() ?? false)
            .First();

        _dbContext.Remove(propertyValue);
        _dbContext.SaveChanges();
    }

    public void UpdateRole(Guid roleId, EntryDef updatedEntry)
    {
        CheckUserChanged();
        new EntryServices().UpdateEntry(roleId, updatedEntry, _dbContext);
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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _dbContext.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return _dbContext.DisposeAsync();
    }

    private void CheckUserChanged()
    {
        if (_userService.CurrentUser != _user)
        {
            _logger.LogInformation("User changed: refreshing DB Context");
            _user = _userService.CurrentUser;
            _dbContext.Dispose();
            _dbContext = _factory.CreateDbContext(_user).Result;
        }
    }
}