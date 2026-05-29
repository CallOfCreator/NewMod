using HarmonyLib;
using NewMod.GeneralEvents;

namespace NewMod.Patches.GeneralEvents
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    public static class StartGECyclePatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
            GeneralEventManager.StartCycle();
        }
    }
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
    public static class StopGECyclePatch
    {
        public static void Postfix(EndGameManager __instance)
        {
            GeneralEventManager.StopCycle();
        }
    }
}