using HarmonyLib;
using NewMod.Utilities;

namespace NewMod.Patches.Roles.EnergyThief;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class OnGameEndPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] EndGameResult endGameResult)
    {
        Utils.ResetDrainCount();
        NewMod.Instance.Log.LogInfo("Reset Drain Count Successfully");
    }
}