using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles;
using NewMod.Utilities;
using Reactor.Utilities;

namespace NewMod.Patches.Roles.Visionary
{
    public static class VisionaryVentEvents
    {
        [RegisterEvent]
        public static void OnEnterVent(EnterVentEvent evt)
        {
            var localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer.Data.Role is not TheVisionary || localPlayer.Data.IsDead || evt.Player == localPlayer || !Helpers.CheckChance(20))
                return;

            var timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            var filePath = System.IO.Path.Combine(VisionaryUtilities.ScreenshotDirectory, $"screenshot_{timestamp}.png");

            Coroutines.Start(Utils.CaptureScreenshot(filePath));
        }

        [RegisterEvent]
        public static void OnExitVent(ExitVentEvent evt)
        {
            var localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer.Data.Role is not TheVisionary || localPlayer.Data.IsDead || evt.Player == localPlayer || !Helpers.CheckChance(20))
                return;

            var timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            var filePath = System.IO.Path.Combine(VisionaryUtilities.ScreenshotDirectory, $"screenshot_{timestamp}.png");

            Coroutines.Start(Utils.CaptureScreenshot(filePath));
        }
    }

    public static class VisionaryMurderEvent
    {
        [RegisterEvent]
        public static void OnBeforeMurder(BeforeMurderEvent evt)
        {
            var localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer.Data.Role is not TheVisionary || localPlayer.Data.IsDead || !Helpers.CheckChance(20))
                return;

            var timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            var filePath = System.IO.Path.Combine(VisionaryUtilities.ScreenshotDirectory, $"screenshot_{timestamp}.png");

            Coroutines.Start(Utils.CaptureScreenshot(filePath));
        }
    }
}