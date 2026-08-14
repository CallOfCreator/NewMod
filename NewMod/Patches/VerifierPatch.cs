using HarmonyLib;
using NewMod.Roles.CrewmateRoles.S1;
using NewMod.Utilities;

namespace NewMod.Patches;

[HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.DoClick))]
public static class VerifierMeetingAbilityPatch
{
    [HarmonyPrefix]
    public static bool Prefix(AbilityButton __instance)
    {
        if (!MeetingHud.Instance)
            return true;

        if (MeetingHud.Instance.MeetingAbilityButton != __instance)
            return true;

        if (!PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.Role is not VerifierRole)
            return true;

        VerifierUtilities.OnMeetingAbilityClicked();
        return false;
    }
}