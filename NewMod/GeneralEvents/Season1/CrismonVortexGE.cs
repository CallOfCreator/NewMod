using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using NewMod.Components;
using NewMod.Components.ScreenEffects;
using NewMod.Options;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewMod.GeneralEvents.Season1;

public class CrismonVortexGE : IGeneralEvent
{
    public static bool Active { get; private set; }
    public static bool PositionReady { get; private set; }
    public static Vector2 VortexPosition { get; private set; }
    public static float StartedAt { get; private set; }

    public string Title => "Crimson Vortex";
    public string Description => "ESCAPE THE SINGULARITY!";
    public LoadableAsset<Sprite> Icon => NewModAsset.CrismonIcon;
    public Color AccentColor => new(0.9f, 0.03f, 0.05f);
    public int OccurrenceChance => (int)MiraAPI.GameOptions.OptionGroupSingleton<global::NewMod.Options.GEOptions>.Instance.CrimsonVortexWeight;

    public float Duration => OptionGroupSingleton<GEOptions>.Instance.CrimsonDuration;

    public bool CanOccur()
    {
        if (!ShipStatus.Instance)
            return false;

        var options = OptionGroupSingleton<GEOptions>.Instance;

        var rooms = ShipStatus.Instance.AllRooms.Where(room => room.RoomId != SystemTypes.Hallway && room.roomArea).ToArray();

        return rooms.Any(vortexRoom => rooms.Any(escapeRoom => escapeRoom != vortexRoom && Vector2.Distance(vortexRoom.roomArea.bounds.center, escapeRoom.roomArea.bounds.center) > options.CrimsonRadius + 1.5f));
    }

    public void OnEventStart()
    {
        Active = true;
        PositionReady = false;

        if (!AmongUsClient.Instance.AmHost)
            return;

        var options = OptionGroupSingleton<GEOptions>.Instance;

        var rooms = ShipStatus.Instance.AllRooms.Where(room => room.RoomId != SystemTypes.Hallway && room.roomArea).ToArray();

        var validVortexRooms = rooms.Where(vortexRoom => rooms.Any(escapeRoom => escapeRoom != vortexRoom && Vector2.Distance(vortexRoom.roomArea.bounds.center, escapeRoom.roomArea.bounds.center) > options.CrimsonRadius + 1.5f)).ToArray();

        var room = validVortexRooms[Random.Range(0, validVortexRooms.Length)];

        var position = (Vector2)room.roomArea.bounds.center;

        RpcSetVortexPosition(PlayerControl.LocalPlayer, position.x, position.y);
    }

    public void OnEventEnd()
    {
        Active = false;
        PositionReady = false;

        if (CrimsonVortexEscapeHud.Instance)
            CrimsonVortexEscapeHud.Instance.ForceHide();

        var effect = Camera.main.GetComponent<CrimsonVortexEffect>();

        if (effect)
            Object.Destroy(effect);
    }

    [MethodRpc((uint)CustomRPC.CrismonVortexPosition, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcSetVortexPosition(PlayerControl source, float x, float y)
    {
        VortexPosition = new Vector2(x, y);
        PositionReady = true;
        StartedAt = Time.time;

        if (!Camera.main.GetComponent<CrimsonVortexEffect>()) Camera.main.gameObject.AddComponent<CrimsonVortexEffect>();
    }

    [MethodRpc((uint)CustomRPC.CrismonVortexEscape, LocalHandling = RpcLocalHandling.After)]
    public static void RpcEscapeVortex(PlayerControl source)
    {
        if (!Active || !PositionReady)
            return;

        var vortex = source.GetComponent<CrismonVortexPhysics>();

        if (vortex && vortex.CanAcceptEscape())
            vortex.BeginEscape();
    }
}
