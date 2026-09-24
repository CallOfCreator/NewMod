using NewMod.RoleLogic;
using System.Linq;
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
        var sample = InjectorUtilities.Experiments.Values.FirstOrDefault(experiment => experiment.TargetId == player.PlayerId);
        if (sample == null || Time.time >= sample.ReadyAt) return;
        var options = OptionGroupSingleton<InjectorOptions>.Instance;
        var change = sample.Serum == SerumType.Adrenaline ? options.AdrenalineSpeedBoost : -options.SedativeSlowdown;
        __instance.body.velocity *= 1f + change / 100f;
    }
}