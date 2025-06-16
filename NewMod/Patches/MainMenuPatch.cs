using HarmonyLib;

namespace NewMod.Patches
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MainMenuPatch
    {
        public static void Postfix(MainMenuManager __instance)
        {
            ModCompatibility.Initialize();
        }
    }
}