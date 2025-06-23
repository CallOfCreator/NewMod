using HarmonyLib;
using MiraAPI.Networking;
using NewMod.Roles.ImpostorRoles;
using NewMod.Utilities;

namespace NewMod.Patches.Roles.Prankster
{
    [HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
    public static class DeadBodyOnClickPatch
    {
        public static bool Prefix(DeadBody __instance)
        {
            var reporter = PlayerControl.LocalPlayer;

            if (!__instance.Reported && PranksterUtilities.IsPranksterBody(__instance))
            {
                reporter.RpcCustomMurder(reporter, true, teleportMurderer:false, showKillAnim:true);

                byte pranksterId = __instance.ParentId;

                PranksterUtilities.IncrementReportCount(pranksterId);

                return false;
            }
            else if (!__instance.Reported && Revenant.FeignDeathStates.TryGetValue(__instance.ParentId, out var feignInfo))
            {
                feignInfo.Reported = true;
                return false;
            }
            return true;
        }
    }
}
