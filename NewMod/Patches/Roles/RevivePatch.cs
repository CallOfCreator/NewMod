using UnityEngine;
using HarmonyLib;

namespace NewMod.Patches.Roles
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Revive))]
    public static class RevivePatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            // Thanks to https://github.com/Dolly1016/Nebula-OLD-/blob/master/NebulaPluginNova/Patches/ButtonsPatch.cs#L75
            __instance.Data.IsDead = false;
            __instance.gameObject.layer = LayerMask.NameToLayer("Players");
            __instance.MyPhysics.ResetMoveState(true);
            __instance.clickKillCollider.enabled = true;
            __instance.cosmetics.SetNameMask(true);

            if (__instance.AmOwner)
            {
                DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(true);
                DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
                DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(false);
            }
            return false;
        }
    }
}