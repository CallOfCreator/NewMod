using System;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;

namespace NewMod
{
    public static class ModCompatibility
    {
        public const string LaunchpadReloaded_GUID = "dev.xtracube.launchpad";
        public static bool IsLaunchpadLoaded()
        {
            return IL2CPPChainloader.Instance.Plugins.ContainsKey(LaunchpadReloaded_GUID);
        }
        public static bool LaunchpadLoaded(out Assembly asm)
        {
            asm = null;
            if (!IL2CPPChainloader.Instance.Plugins.TryGetValue(LaunchpadReloaded_GUID, out var lp)) return false;
            asm = lp.Instance.GetType().Assembly;
            return asm != null;
        }
        public static void Initialize()
        {
            if (!IsLaunchpadLoaded()) return;

            NewMod.Instance.Log.LogMessage("LaunchpadReloaded detected. Enabling compatibility...");
        }
        public static void DisableRole(string roleName, string pluginGuid)
        {
            var plugin = MiraPluginManager.GetPluginByGuid(pluginGuid);
            if (plugin == null) return;

            foreach (var kv in plugin.Roles)
            {
                var role = kv.Value;

                if (role is ICustomRole customRole && customRole.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var config = customRole.Configuration;
                        customRole.SetChance(0);
                        customRole.SetCount(0);
                        customRole.ParentMod.PluginConfig.Save();
                        return;
                    }
                    catch (Exception e)
                    {
                        NewMod.Instance.Log.LogError($"Failed to disable role '{roleName}': {e.Message}");
                    }
                }
            }
        }
        public static bool IsRoleActive(string roleName)
        {
            foreach (var roles in RoleManager.Instance.AllRoles)
            {
                CustomRoleManager.GetCustomRoleBehaviour(roles.Role, out var customRole);

                if (customRole != null && customRole.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
                {
                    return customRole.GetChance() > 0 && customRole.GetCount() > 0;
                }
            }
            return false;
        }
    }
}
