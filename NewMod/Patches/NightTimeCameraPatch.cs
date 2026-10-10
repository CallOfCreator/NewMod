using NewMod.Components.ScreenEffects.Effects;
using HarmonyLib;
using NewMod.Components.ScreenEffects;
using UnityEngine;

namespace NewMod.Patches;

[HarmonyPatch]
public static class NightTimeCameraPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
    public static void SkeldCameras(SurveillanceMinigame __instance)
    {
        foreach (var camera in __instance.GetComponentsInChildren<Camera>(true))
            if (camera.targetTexture)
                NightTimeCameraEffect.Attach(camera);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Begin))]
    public static void PlanetCameras(PlanetSurveillanceMinigame __instance)
    {
        NightTimeCameraEffect.Attach(__instance.Camera);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Begin))]
    public static void FungleBinoculars(FungleSurveillanceMinigame __instance)
    {
        NightTimeCameraEffect.Attach(__instance.securityCamera.cam);
    }
}
