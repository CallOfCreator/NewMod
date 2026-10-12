using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using NewMod.GeneralEvents.Season1;
using NewMod.Networking;
using NewMod.Roles.CrewmateRoles;
using NewMod.Utilities;
using UnityEngine;

namespace NewMod.Patches.Roles.Visionary;

public static class VisionaryMeetingEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
            VisionaryUtilities.DeleteAllScreenshots();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        VisionaryUtilities.MeetingNumber++;
        VisionaryUtilities.BroadcastOwners.Clear();
        Object.Destroy(VisionaryUtilities.PhotoPanel);
        VisionaryUtilities.PhotoPanel = null;
        UpdateMeetingButton();
    }

    public static void UpdateMeetingButton()
    {
        if (!MeetingHud.Instance)
            return;

        var button = MeetingHud.Instance.MeetingAbilityButton;
        if (!AbilityExchangeGE.HasRoleAbility<TheVisionary>(PlayerControl.LocalPlayer) || PlayerControl.LocalPlayer.Data.IsDead)
        {
            if (button.graphic.sprite == NewModAsset.ShowScreenshotButton.LoadAsset())
                button.Hide();
            return;
        }
        button.Show();
        button.SetInfiniteUses();
        button.SetCoolDown(0f, 1f);
        button.graphic.sprite = NewModAsset.ShowScreenshotButton.LoadAsset();
        button.OverrideColor(Color.white);
        button.OverrideText(VisionaryUtilities.BroadcastOwners.Contains(PlayerControl.LocalPlayer.PlayerId) ? "SENT" : "BROADCAST");
    }

    [RegisterEvent]
    public static void OnMeetingEnd(EndMeetingEvent evt)
    {
        Object.Destroy(VisionaryUtilities.PhotoPanel);
        VisionaryUtilities.PhotoPanel = null;
        foreach (var key in VisionaryPhotoRpc.Transfers.Keys.ToArray())
        {
            if (key.Meeting >= 0)
                VisionaryPhotoRpc.Transfers.Remove(key);
        }
    }
}
