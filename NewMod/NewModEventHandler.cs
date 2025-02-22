using System.Collections.Generic;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Usables;
using NewMod.Patches;
using NewMod.Patches.Roles.Visionary;

namespace NewMod
{
    public static class NewModEventHandler
    {
        public static void RegisterAllEvents()
        {
            var registrations = new List<string>();

            MiraEventManager.RegisterEventHandler<GameEndEvent>(EndGamePatch.OnGameEnd, 1);
            registrations.Add($"{nameof(GameEndEvent)}: {nameof(EndGamePatch.OnGameEnd)}");

            MiraEventManager.RegisterEventHandler<EnterVentEvent>(VisionaryVentPatch.OnEnterVent);
            registrations.Add($"{nameof(EnterVentEvent)}: {nameof(VisionaryVentPatch.OnEnterVent)}");

            MiraEventManager.RegisterEventHandler<ExitVentEvent>(VisionaryVentPatch.OnExitVent);
            registrations.Add($"{nameof(ExitVentEvent)}: {nameof(VisionaryVentPatch.OnExitVent)}");

            MiraEventManager.RegisterEventHandler<BeforeMurderEvent>(VisionaryMurderPatch.OnBeforeMurder);
            registrations.Add($"{nameof(BeforeMurderEvent)}: {nameof(VisionaryMurderPatch.OnBeforeMurder)}");

            NewMod.Instance.Log.LogInfo("Registered events: " + string.Join(", ", registrations));
        }
    }
}
