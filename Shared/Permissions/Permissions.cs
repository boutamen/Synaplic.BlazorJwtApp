using System.Reflection;

namespace Synaplic.BlazorJwtApp.Shared.Permissions
{
    public static class Permissions
    {
        // 🔹 Define Short Codes for Permissions (reduces JWT size)
        public const string ManageUsers = "MU";
        public const string ManageRoles = "MR";
        public const string ViewReports = "VR";
        public const string EditSettings = "ES";
        public const string AccessAdminPanel = "AAP";
        public const string GetAdminWeather = "GAW";
        public const string GetBasicWeather = "GBW";

        // 🔹 Retrieve all permissions dynamically
        public static readonly List<string> All = typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly) // Only constants
            .Select(fi => fi.GetValue(null)?.ToString())
            .Where(value => value != null)
            .ToList()!;

        // 🔹 Automatically Generate Basic Permissions
        public static readonly List<string> Basics = All.Where(p => p is "GBW" or "VR").ToList();
    }
}
