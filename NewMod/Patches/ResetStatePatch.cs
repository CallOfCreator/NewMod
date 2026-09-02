using HarmonyLib;

namespace NewMod.Patches;

public static class ResetStatePatch
{
    // Thanks to: https://github.com/AU-Avengers/TOU-Mira/blob/main/TownOfUs/Patches/CancelCountdownStartPatches.cs#L120

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.ResetStartState))]
    public static void Prefix(GameStartManager __instance)
    {
        if (!AmongUsClient.Instance.AmHost || __instance.startState != GameStartManager.StartingStates.Countdown || GameManager.Instance is not { } gameManager)
            return;

        gameManager.LogicOptions.SyncOptions();
    }
}