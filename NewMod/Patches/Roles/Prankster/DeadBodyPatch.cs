using HarmonyLib;
using MiraAPI.Networking;
using NewMod.Utilities;

namespace NewMod.Patches.Roles.Prankster
{
    [HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
    public static class DeadBodyOnClickPatch
    {
        public static bool Prefix(DeadBody __instance)
        {
            if (!__instance.Reported && PranksterUtilities.IsPranksterBody(__instance))
            {
                var reporter = PlayerControl.LocalPlayer;
            
                reporter.RpcCustomMurder(reporter, true, teleportMurderer:false, showKillAnim:true);

                byte pranksterId = __instance.ParentId;

                PranksterUtilities.IncrementReportCount(pranksterId);

                return false;
            }
            return true;
        }
    }
}
