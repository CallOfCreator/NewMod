using HarmonyLib;
using NewMod.Roles.NeutralRoles.S1;
using UnityEngine;

namespace NewMod.Patches.Roles;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
public static class BountyMovementPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(PlayerPhysics __instance)
    {
        var player = __instance.myPlayer;
        if (!player.AmOwner || player.Data.IsDead || player.Data.Role is not Bounty || MeetingHud.Instance || ExileController.Instance)
            return;

        if (Bounty.Contracts.TryGetValue(player.PlayerId, out var contract) && contract.Phase == Bounty.ContractPhase.Collection && Bounty.CollectionStartsAt.TryGetValue(player.PlayerId, out var startsAt) && Time.time < startsAt)
            __instance.body.velocity = Vector2.zero;
    }
}
