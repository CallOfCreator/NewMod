using HarmonyLib;
using NewMod.Components.ScreenEffects;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class HudPatch
    {
        public static void Postfix(HudManager __instance)
        {
            // for some reasons the effect si getting destroyed so the best way to make it alive is to reassign it here
            ShadowFluxEffect._shader = NewModAsset.ShadowFluxShader.LoadAsset();
        }
    }
}