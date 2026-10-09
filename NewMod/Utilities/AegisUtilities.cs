using NewMod.Modifiers.S1;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using NewMod.Components;
using NewMod.Options.Roles;
using NewMod.Roles.CrewmateRoles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace NewMod.Utilities;

public static class AegisUtilities
{
    [MethodRpc((uint)CustomRPC.AegisRequestWard, LocalHandling = RpcLocalHandling.After)]
    public static void RpcRequestWard(PlayerControl source)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Aegis || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ShieldArea._active.Any(area => area.ownerId == source.PlayerId))
            return;
        OverclockedModifier.Pulse(source);
        var position = source.GetTruePosition();
        RpcPlaceWard(PlayerControl.LocalPlayer, source.PlayerId, position.x, position.y);
    }

    [MethodRpc((uint)CustomRPC.AegisPlaceWard, LocalHandling = RpcLocalHandling.After)]
    public static void RpcPlaceWard(PlayerControl source, byte ownerId, float x, float y)
    {
        if (!source.IsHost())
            return;
        var options = OptionGroupSingleton<AegisOptions>.Instance;
        var go = new GameObject("AegisWard");
        go.transform.SetParent(ShipStatus.Instance.transform, false);
        go.transform.position = new Vector2(x, y);
        go.AddComponent<ShieldArea>().Init(ownerId, options.Radius, options.DurationSeconds);
    }

    [MethodRpc((uint)CustomRPC.AegisBreakWard, LocalHandling = RpcLocalHandling.After)]
    public static void RpcBreakWard(PlayerControl source, byte ownerId)
    {
        if (!source.IsHost())
            return;

        var area = ShieldArea._active.FirstOrDefault(ward => ward.ownerId == ownerId);
        if (area)
        {
            ShieldArea._active.Remove(area);
            Object.Destroy(area.gameObject);
        }
    }
}