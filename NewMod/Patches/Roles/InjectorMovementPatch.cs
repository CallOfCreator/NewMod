using NewMod.Roles.NeutralRoles;
using HarmonyLib;
using MiraAPI.GameOptions;
using NewMod.Options.Roles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Patches.Roles;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
public static class InjectorMovementPatch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerPhysics __instance)
    {
        var player = __instance.myPlayer;
        if (!player.AmOwner || !player.CanMove) return;
        foreach (var sample in InjectorUtilities.Experiments.Values)
        {
            if (sample.TargetId != player.PlayerId || Time.time >= sample.ReadyAt)
                continue;

            var options = OptionGroupSingleton<InjectorOptions>.Instance;
            var change = sample.Serum == InjectorRole.SerumType.Adrenaline ? options.AdrenalineSpeedBoost : -options.SedativeSlowdown;
            __instance.body.velocity *= 1f + change / 100f;
            break;
        }
    }
}