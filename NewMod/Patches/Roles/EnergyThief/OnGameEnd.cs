using HarmonyLib;
using NewMod.Utilities;

namespace NewMod.Patches.Roles.EnergyThief;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class OnGameEndPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] EndGameResult endGameResult)
    {
        Utils.ResetDrainCount();
        PranksterUtilities.ResetReportCount();
        Utils.ResetMissionSuccessCount();
        Utils.ResetMissionFailureCount();
        NewMod.Instance.Log.LogInfo("Reset Drain Count Successfully");
        NewMod.Instance.Log.LogInfo("Reset Clone Report Count Successfully");
        NewMod.Instance.Log.LogInfo("Reset Mission Success Count Successfully");
        NewMod.Instance.Log.LogInfo("Reset Mission Failure Count Successfully");
    }
}