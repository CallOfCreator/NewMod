using System.Reflection;
using NewMod.Roles.ImpostorRoles;
using TMPro;
using UnityEngine;

namespace NewMod.Patches.Compatibility;

public static class LaunchpadCompatibility
{
    public static MethodInfo TargetMethod()
    {
        if (!ModCompatibility.LaunchpadLoaded(out var asm) || asm == null)
            return null;

        var type = asm.GetType("LaunchpadReloaded.Modifiers.HackedModifier");
        var method = type?.GetMethod("OnTimerComplete", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return method;
    }

    public static bool Prefix(object __instance)
    {
        var playerField = __instance.GetType().GetField("Player", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (playerField == null) return true;

        var player = playerField.GetValue(__instance) as PlayerControl;

        if (player != null && Revenant.Phases.TryGetValue(player.PlayerId, out var phase) && phase == Revenant.Phase.Feigning)
        {
            Info($"Blocked Launchpad hack death on Revenant {player.Data.PlayerName}");
            return false;
        }

        return true;
    }
}

public static class LaunchpadHackTextPatch
{
    public static MethodInfo TargetMethod()
    {
        if (!ModCompatibility.LaunchpadLoaded(out var asm) || asm == null)
            return null;

        var type = asm.GetType("LaunchpadReloaded.Modifiers.HackedModifier");
        var method = type?.GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return method;
    }

    public static void Postfix(object __instance)
    {
        var player = __instance.GetType().GetField("Player", BindingFlags.Instance | BindingFlags.Public)?.GetValue(__instance) as PlayerControl;
        var hackedText = __instance.GetType().GetField("_hackedText", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(__instance) as TextMeshPro;

        if (player != null && hackedText != null && Revenant.Phases.TryGetValue(player.PlayerId, out var phase) && phase == Revenant.Phase.Feigning)
        {
            hackedText.SetText(string.Empty);
            Debug($"hackedText: {hackedText.text}");
        }
    }
}

public static class LaunchpadTagSpacingPatch
{
    public static MethodInfo TargetMethod()
    {
        if (!ModCompatibility.LaunchpadLoaded(out var asm) || asm == null)
            return null;

        var type = asm.GetType("LaunchpadReloaded.Components.PlayerTagManager");
        var method = type?.GetMethod("UpdatePosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return method;
    }

    public static void Postfix(object __instance)
    {
        var type = __instance.GetType();
        var tagHolderObj = type.GetField("tagHolder", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(__instance);
        if (tagHolderObj is Transform holder)
        {
            holder.localPosition = new Vector3(0f, 0.5491f, -0.35f);
            holder.localScale = new Vector3(0.7455f, 1f, 1f);
        }
    }
}
