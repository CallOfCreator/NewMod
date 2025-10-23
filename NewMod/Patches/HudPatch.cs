using HarmonyLib;
using NewMod.Components.ScreenEffects;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class HudPatch
    {
        public static void Postfix(HudManager __instance)
        {
           // For some reason the effect gets destroyed, so the best way to keep it persistent is to reassign it here
            ShadowFluxEffect._shader = NewModAsset.ShadowFluxShader.LoadAsset();
        }
    }
}