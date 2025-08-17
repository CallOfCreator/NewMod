using MiraAPI.Roles;
using NewMod.Roles;

namespace NewMod.Extensions
{
    public static class RoleExtensions
    {
        public static bool IsNewModRoleFaction(this ICustomRole role) => role is INewModRole;
    }
}