using System.Collections;
using System.Linq;
using MiraAPI.Utilities.Assets;
using NewMod.Components.ScreenEffects;
using NewMod.Components.ScreenEffects.Effects;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.GeneralEvents.Season1;

public class SystemOverrideGE : IGeneralEvent
{
    public IEnumerator OverrideRoutine;
    public static bool Active { get; private set; }
    public string Title => "System Override";
    public string Description => "SHIP SYSTEMS ARE BEING OVERRIDDEN!";
    public LoadableAsset<Sprite> Icon => NewModAsset.SystemOverrideIcon;
    public Color AccentColor => new(1f, 0.16f, 0.2f);
    public int OccurrenceChance => (int)MiraAPI.GameOptions.OptionGroupSingleton<Options.GEOptions>.Instance.SystemOverrideFrequency.Value;
    public float Duration => 22f;

    public bool CanOccur()
    {
        return true;
    }

    public void OnEventStart()
    {
        Active = true;

        var camera = Camera.main;
        if (camera)
            camera.AddScreenEffect<SystemOverrideEffect>();

        if (AmongUsClient.Instance.AmHost) OverrideRoutine = Coroutines.Start(CoOverrideSystems());
    }

    public void OnEventEnd()
    {
        Active = false;

        Coroutines.Stop(OverrideRoutine);
        OverrideRoutine = null;

        var camera = Camera.main;
        if (!camera)
            return;

        var effect = camera.GetScreenEffect<SystemOverrideEffect>();
        if (effect != null)
            effect.Remove();
    }

    public static IEnumerator CoOverrideSystems()
    {
        while (Active)
        {
            yield return new WaitForSeconds(Random.Range(3.2f, 5.2f));

            if (!Active)
                yield break;

            if (MeetingHud.Instance || ExileController.Instance)
                continue;

            var rooms = ShipStatus.Instance.AllRooms.Where(room => room.RoomId != SystemTypes.Hallway).ToArray();

            if (rooms.Length == 0)
                continue;

            ShipStatus.Instance.RpcCloseDoorsOfType(rooms[Random.Range(0, rooms.Length)].RoomId);
        }
    }
}
