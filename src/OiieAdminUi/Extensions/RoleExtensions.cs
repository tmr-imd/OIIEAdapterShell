using CIRLib.ObjectModel.Models;

namespace OiieAdminUi.Extensions;

public static class RoleExtensions
{
    public static string GetPrimaryName(this Entry role)
    {
        return string.IsNullOrWhiteSpace(role.Name) ?
            (string.IsNullOrWhiteSpace(role.IdInSource) ? "Name/Id Missing" : role.IdInSource)
            : role.Name;
    }

    public static string GetSecondaryName(this Entry role)
    {
        return string.IsNullOrWhiteSpace(role.Name) ? "" : $"({role.IdInSource})";
    }

    public static string GetPrimaryName(this Category system)
    {
        return string.IsNullOrWhiteSpace(system.Description) ?
            (string.IsNullOrWhiteSpace(system.CategorySourceId) ? "Name/Id Missing" : system.CategorySourceId)
            : system.Description;
    }

    public static string GetSecondaryName(this Category system)
    {
        return string.IsNullOrWhiteSpace(system.Description) ? "" : $"({system.CategorySourceId})";
    }
}