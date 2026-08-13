using HarmonyLib;

namespace NewMod.Patches;

public static class GameStartPatch
{
    // Thanks to: https://github.com/AU-Avengers/TOU-Mira/blob/main/TownOfUs/Patches/CancelCountdownStartPatches.cs#L120

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.ResetStartState))]
    public static void Prefix(GameStartManager __instance)
    {
        if (__instance.startState == GameStartManager.StartingStates.Countdown)
        {
            GameManager.Instance.LogicOptions.SyncOptions();
        }
    }
}