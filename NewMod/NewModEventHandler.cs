using System.Collections.Generic;
using System.Linq;
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

            MiraEventManager.RegisterEventHandler<BeforeMurderEvent>(VisionaryMurderPatch.OnBeforeMurder);
            registrations.Add($"{nameof(BeforeMurderEvent)}: {nameof(VisionaryMurderPatch.OnBeforeMurder)}");

            MiraEventManager.RegisterEventHandler<AfterMurderEvent>(NewMod.OnAfterMurder);
            registrations.Add($"{nameof(AfterMurderEvent)}: {nameof(NewMod.OnAfterMurder)}");

            NewMod.Instance.Log.LogInfo("Registered events: " + "\n" + string.Join(", ", registrations));
        }
    }
}
