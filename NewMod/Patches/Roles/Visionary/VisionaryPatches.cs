using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
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
        if (PlayerControl.LocalPlayer.Data.Role is not TheVisionary || PlayerControl.LocalPlayer.Data.IsDead)
            return;
        var button = evt.MeetingHud.MeetingAbilityButton;
        button.Show();
        button.SetInfiniteUses();
        button.SetCoolDown(0f, 1f);
        button.graphic.sprite = NewModAsset.ShowScreenshotButton.LoadAsset();
        button.OverrideText("BROADCAST");
    }

    [RegisterEvent]
    public static void OnMeetingEnd(EndMeetingEvent evt)
    {
        Object.Destroy(VisionaryUtilities.PhotoPanel);
        VisionaryUtilities.PhotoPanel = null;
        foreach (var key in System.Linq.Enumerable.ToArray(VisionaryPhotoRpc.Transfers.Keys))
        {
            if (key.Meeting >= 0)
                VisionaryPhotoRpc.Transfers.Remove(key);
        }
    }
}
