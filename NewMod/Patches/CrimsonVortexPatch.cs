using HarmonyLib;
using NewMod.Components;
using UnityEngine;

namespace NewMod.Patches;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.Awake))]
public static class CrismonVortexPhysicsAwakePatch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerPhysics __instance)
    {
        if (!__instance.GetComponent<CrismonVortexPhysics>()) __instance.gameObject.AddComponent<CrismonVortexPhysics>();
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.SetNormalizedVelocity))]
public static class CrismonVortexFixedUpdatePatch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerPhysics __instance, Vector2 direction)
    {
        if (!__instance.AmOwner)
            return;

        var vortex = __instance.GetComponent<CrismonVortexPhysics>();

        if (vortex)
            vortex.ApplyVortex();
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class CrimsonVortexEscapeHudPatch
{
    [HarmonyPostfix]
    public static void Postfix(HudManager __instance)
    {
        if (!__instance.GetComponent<CrimsonVortexEscapeHud>()) __instance.gameObject.AddComponent<CrimsonVortexEscapeHud>();
    }
}

[HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
public static class CrimsonVortexUseButtonPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        var vortex = PlayerControl.LocalPlayer.GetComponent<CrismonVortexPhysics>();

        if (!vortex || !vortex.ShouldConsumeEscapeInput())
            return true;

        return Application.platform != RuntimePlatform.Android && !Input.GetKeyDown(KeyCode.Space);
    }
}