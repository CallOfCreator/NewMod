using HarmonyLib;
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
                reporter.RpcMurderPlayer(reporter, true);
                return false;
            }
            return true;
        }
    }
}
