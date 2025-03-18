using HarmonyLib;
using MiraAPI.Hud;
using NewMod.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Patches.Roles.EnergyThief;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class OnGameEndPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] EndGameResult endGameResult)
    {
        Utils.ResetDrainCount();
        Utils.ResetMissionSuccessCount();
        Utils.ResetMissionFailureCount();
        PranksterUtilities.ResetReportCount();
        VisionaryUtilities.DeleteAllScreenshots();
        Revenant.HasUsedFeignDeath = false;
        Revenant.FeignDeathStates.Remove(PlayerControl.LocalPlayer.PlayerId);
        Revenant.StalkingStates[PlayerControl.LocalPlayer.PlayerId] = false;
        NewMod.Instance.Log.LogInfo("Reset Drain Count Successfully");
        NewMod.Instance.Log.LogInfo("Reset Clone Report Count Successfully");
        NewMod.Instance.Log.LogInfo("Reset Mission Success Count Successfully");
        NewMod.Instance.Log.LogInfo("Reset Mission Failure Count Successfully");
        NewMod.Instance.Log.LogInfo("Deleted all Visionary's screenshots Successfully");
    }
}