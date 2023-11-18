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
}