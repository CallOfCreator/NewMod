using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using NewMod.GeneralEvents.Season1;
using NewMod.Roles.CrewmateRoles;
using NewMod.Roles.CrewmateRoles.S1;
using NewMod.Roles.NeutralRoles;
using NewMod.Roles.NeutralRoles.S1;
using NewMod.Utilities;

namespace NewMod.Patches;

[HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.DoClick))]
public static class MeetingAbilityPatch
{
    [RegisterEvent]
    public static void OnButtonClick(MiraButtonClickEvent evt)
    {
        if (MeetingHud.Instance || ExileController.Instance)
            evt.Cancel();
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool Prefix(AbilityButton __instance)
    {
        if (ExileController.Instance) return false;
        if (!MeetingHud.Instance) return true;
        if (MeetingHud.Instance.MeetingAbilityButton != __instance || !PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data) return false;

        switch (PlayerControl.LocalPlayer.Data.Role)
        {
            case RoleBehaviour _ when AbilityExchangeGE.HasRoleAbility<TheVisionary>(PlayerControl.LocalPlayer):
                if (VisionaryUtilities.SelectedCamera >= 0 && !VisionaryUtilities.BroadcastOwners.Contains(PlayerControl.LocalPlayer.PlayerId))
                {
                    VisionaryUtilities.RpcBroadcast(PlayerControl.LocalPlayer, VisionaryUtilities.SelectedCamera);
                    VisionaryUtilities.BroadcastOwners.Add(PlayerControl.LocalPlayer.PlayerId);
                    __instance.OverrideText("SENT");
                }

                return false;

            case VerifierRole:
                VerifierUtilities.OnMeetingAbilityClicked();
                return false;

            case RoleBehaviour _ when AbilityExchangeGE.HasRoleAbility<ArbitratorRole>(PlayerControl.LocalPlayer):
                ArbitratorRole.OnMeetingAbilityClicked();
                return false;

            case EgoistRole:
                EgoistRole.OnMeetingAbilityClicked();
                return false;

            default:
                return true;
        }
    }
}
