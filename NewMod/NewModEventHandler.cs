using System.Collections.Generic;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Usables;
using NewMod.Patches;
using NewMod.Patches.Roles.Visionary;

namespace NewMod
{
    public static class NewModEventHandler
    {
        public static void RegisterEventsLogs()
        {
            var registrations = new List<string>
            {
                $"{nameof(GameEndEvent)}: {nameof(EndGamePatch.OnGameEnd)}",
                $"{nameof(EnterVentEvent)}: {nameof(VisionaryVentPatch.OnEnterVent)}",
                $"{nameof(BeforeMurderEvent)}: {nameof(VisionaryMurderPatch.OnBeforeMurder)}",
                $"{nameof(AfterMurderEvent)}: {nameof(NewMod.OnAfterMurder)}"
            };
            NewMod.Instance.Log.LogInfo("Registered events: " + "\n" + string.Join(", ", registrations));
        }
    }
}
