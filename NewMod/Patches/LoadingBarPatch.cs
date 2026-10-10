using HarmonyLib;
using NewMod.Components;

namespace NewMod.Patches;

[HarmonyPatch(typeof(AmongUsLoadingBar))]
public static class LoadingBarPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(AmongUsLoadingBar.OnEnable))]
    public static void OnEnable(AmongUsLoadingBar __instance)
    {
        var loading = __instance.barFill.GetComponent<NewModLoadingBar>();
        if (!loading)
            loading = __instance.barFill.gameObject.AddComponent<NewModLoadingBar>();
        loading.Initialize(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(AmongUsLoadingBar.SetLoadingPercent))]
    public static void SetLoadingPercent(AmongUsLoadingBar __instance, float percent)
    {
        var loading = __instance.barFill.GetComponent<NewModLoadingBar>();
        if (loading)
            loading.SetProgress(percent);
    }
}
