using System;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using NewMod.Cosmetics;
using NewMod.Patches.Compatibility;
using UnityEngine;

namespace NewMod;

public static class ModCompatibility
{
    public static bool CorsacCosmeticsEnabled => Application.platform != RuntimePlatform.Android && IL2CPPChainloader.Instance.Plugins.ContainsKey(NewMod.CorsacPluginId);

    public static bool IsLaunchpadLoaded()
    {
        return IL2CPPChainloader.Instance.Plugins.ContainsKey(NewMod.LaunchpadReloadedId);
    }
    
    public static bool LaunchpadLoaded(out Assembly asm)
    {
        asm = null;
        if (!IL2CPPChainloader.Instance.Plugins.TryGetValue(NewMod.LaunchpadReloadedId, out var lp)) return false;
        asm = lp.Instance.GetType().Assembly;
        return asm != null;
    }

    public static void Initialize()
    {
        if (CorsacCosmeticsEnabled)
        {
            CorsacCosmeticsIntegration.Initialize(NewMod.Harmony);
            
            Message("CorsacCosmetics detected. Enabling comsmetics...");
        }
        
        if (IsLaunchpadLoaded())
        {
            NewMod.Harmony.PatchAll(typeof(LaunchpadCompatibility));
            NewMod.Harmony.PatchAll(typeof(LaunchpadHackTextPatch));
            
            Message("LaunchpadReloaded detected. Enabling compatibility...");
        }
    }

    public static void DisableRole(string roleName, string pluginGuid)
    {
        var plugin = MiraPluginManager.GetPluginByGuid(pluginGuid);
        if (plugin == null) return;

        foreach (var kv in plugin.Roles)
        {
            var role = kv.Value;

            if (role is ICustomRole customRole && customRole.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
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
                    Error($"Failed to disable role '{roleName}': {e.Message}");
                }
        }
    }
}