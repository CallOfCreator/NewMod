using HarmonyLib;
using NewMod.Roles.ImpostorRoles;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace NewMod.Patches.Compatibility
{
    [HarmonyPatch]
    public static class LaunchpadCompatibility
    {
        static MethodBase TargetMethod()
        {
            if (!ModCompatibility.LaunchpadLoaded(out var asm))
            {
                return null;
            }

            var type = asm.GetType("LaunchpadReloaded.Modifiers.HackedModifier");
            var method = type?.GetMethod("OnTimerComplete", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return method;
        }

        static bool Prefix(object __instance)
        {
            var playerField = __instance.GetType().GetField("Player", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (playerField == null) return true;

            var player = playerField.GetValue(__instance) as PlayerControl;

            if (player != null && Revenant.FeignDeathStates.ContainsKey(player.PlayerId))
            {
                NewMod.Instance.Log.LogInfo($"Blocked Launchpad hack death on Revenant {player.Data.PlayerName}");
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch]
    public static class LaunchpadHackTextPatch
    {
        static MethodBase TargetMethod()
        {
            if (!ModCompatibility.LaunchpadLoaded(out var asm))
            {
                return null;
            }

            var type = asm.GetType("LaunchpadReloaded.Modifiers.HackedModifier");
            var method = type?.GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return method;
        }

        static void Postfix(object __instance)
        {
            var player = __instance.GetType().GetField("Player", BindingFlags.Instance | BindingFlags.Public)?.GetValue(__instance) as PlayerControl;
            var hackedText = __instance.GetType().GetField("_hackedText", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(__instance) as TextMeshPro;

            if (player != null && hackedText != null && Revenant.FeignDeathStates.ContainsKey(player.PlayerId))
            {
                hackedText.SetText("");
                Debug.Log($"hackedText: {hackedText.text}");
            }
        }
    }
}
