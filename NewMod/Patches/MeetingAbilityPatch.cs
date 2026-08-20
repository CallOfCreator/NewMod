using HarmonyLib;
using NewMod.Roles.CrewmateRoles.S1;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.Utilities;

namespace NewMod.Patches;

[HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.DoClick))]
public static class MeetingAbilityPatch
{
    [HarmonyPrefix]
    public static bool Prefix(AbilityButton __instance)
    {
        if (!MeetingHud.Instance || MeetingHud.Instance.MeetingAbilityButton != __instance)
        {
            return true;
        }

        switch (PlayerControl.LocalPlayer.Data.Role)
        {
            case VerifierRole:
                VerifierUtilities.OnMeetingAbilityClicked();
                return false;

            case ArbitratorRole:
                ArbitratorRole.OnMeetingAbilityClicked();
                return false;

            default:
                return true;
        }
    }
}