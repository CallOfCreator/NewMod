using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles.S1;

[MiraIgnore]
public sealed class Nomad : CrewmateRole, INewModRole
{
    public static readonly Dictionary<byte, Vector2> Anchors = [];
    public static readonly Dictionary<byte, int> LastRooms = [];
    public static readonly HashSet<byte> CanWander = [];
    public static readonly Dictionary<byte, Vector2> BacktrackPositions = [];

    private static readonly Dictionary<byte, int> Scores = [];
    private static readonly Dictionary<byte, HashSet<byte>> RouteRooms = [];
    private static readonly Dictionary<byte, Vector2> RouteEnds = [];

    public string RoleName => "Nomad";
    public string RoleDescription => "Complete room routes to arm Backtrack.";
    public string RoleLongDescription => "Anchor a room, travel through unique rooms, then Wander home. Each completed route arms Backtrack, blocking one murder and returning you to the route's far end.";
    public Color RoleColor => new(0.22f, 0.78f, 0.92f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public NewModFaction Faction => NewModFaction.Rift;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            MaxRoleCount = 1,
            DefaultRoleCount = 1,
            DefaultChance = 35,
            CanModifyChance = true,
            CanUseSabotage = false,
            CanUseVent = false,
            UseVanillaKillButton = false,
            TasksCountForProgress = false,
            Icon = MiraAssets.Empty
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = INewModRole.GetRoleTabText(this);
        var playerId = PlayerControl.LocalPlayer.PlayerId;
        Scores.TryGetValue(playerId, out var score);

        text.AppendLine($"<size=65%>Routes: <color=#65D5EB>{score}</color>/{(int)OptionGroupSingleton<NomadOptions>.Instance.ScoreGoal}</size>");
        text.AppendLine(CanWander.Contains(playerId) ? "<size=65%><color=#75E6A5>Wander ready.</color></size>" : "<size=65%><color=#A4A4A4>Keep moving through new rooms.</color></size>");
        if (BacktrackPositions.ContainsKey(playerId))
            text.AppendLine("<size=65%><color=#78E8FF>Backtrack armed: the next normal murder returns you to your route.</color></size>");
        return text;
    }

    public override bool DidWin(GameOverReason reason)
    {
        return reason == CustomGameOver.GameOverReason<NomadGameOver>();
    }

    public static void FixedUpdate(PlayerControl player)
    {
        if (player.Data.IsDead || !Anchors.ContainsKey(player.PlayerId))
            return;

        foreach (var room in ShipStatus.Instance.AllRooms)
        {
            if (room.RoomId == SystemTypes.Hallway || !room.roomArea.OverlapPoint(player.GetTruePosition()))
                continue;

            var roomId = (byte)room.RoomId;
            if (!LastRooms.TryGetValue(player.PlayerId, out var lastRoom) || lastRoom != roomId)
            {
                var position = player.GetTruePosition();
                RpcConfirmRoom(PlayerControl.LocalPlayer, player.PlayerId, roomId, position.x, position.y);
            }

            return;
        }
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;

        Anchors.Clear();
        LastRooms.Clear();
        CanWander.Clear();
        BacktrackPositions.Clear();
        Scores.Clear();
        RouteRooms.Clear();
        RouteEnds.Clear();
    }

    [RegisterEvent]
    public static void OnSetRole(SetRoleEvent evt)
    {
        if (evt.Player.Data.Role is Nomad)
            return;

        var playerId = evt.Player.PlayerId;
        Anchors.Remove(playerId);
        LastRooms.Remove(playerId);
        CanWander.Remove(playerId);
        BacktrackPositions.Remove(playerId);
        Scores.Remove(playerId);
        RouteRooms.Remove(playerId);
        RouteEnds.Remove(playerId);
    }

    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost || evt.IgnoreDefense || evt.Target.Data.Role is not Nomad || !BacktrackPositions.TryGetValue(evt.Target.PlayerId, out var position))
            return;

        evt.Cancel();
        RpcConfirmBacktrack(PlayerControl.LocalPlayer, evt.Target.PlayerId, position.x, position.y);
    }

    [RegisterEvent]
    public static void OnMeetingEnd(EndMeetingEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        foreach (var pair in Scores)
        {
            var player = Utils.PlayerById(pair.Key);
            if (!player.Data.IsDead && !player.Data.Disconnected && pair.Value >= OptionGroupSingleton<NomadOptions>.Instance.ScoreGoal)
            {
                CustomGameOver.Trigger<NomadGameOver>([player.Data]);
                return;
            }
        }
    }

    [MethodRpc((uint)CustomRPC.NomadRequestAnchor)]
    public static void RpcRequestAnchor(PlayerControl source, byte roomId, float x, float y)
    {
        if (AmongUsClient.Instance.AmHost && source.Data.Role is Nomad && !source.Data.IsDead)
            RpcConfirmAnchor(PlayerControl.LocalPlayer, source.PlayerId, roomId, x, y);
    }

    [MethodRpc((uint)CustomRPC.NomadConfirmAnchor, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmAnchor(PlayerControl host, byte playerId, byte roomId, float x, float y)
    {
        if (!host.IsHost())
            return;

        Anchors[playerId] = new Vector2(x, y);
        LastRooms[playerId] = roomId;
        RouteRooms[playerId] = [roomId];
        RouteEnds[playerId] = new Vector2(x, y);
        CanWander.Remove(playerId);
    }

    [MethodRpc((uint)CustomRPC.NomadConfirmRoom, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmRoom(PlayerControl host, byte playerId, byte roomId, float x, float y)
    {
        if (!host.IsHost() || !RouteRooms.TryGetValue(playerId, out var rooms))
            return;

        LastRooms[playerId] = roomId;
        if (!rooms.Add(roomId))
        {
            RouteRooms.Remove(playerId);
            RouteEnds.Remove(playerId);
            Anchors.Remove(playerId);
            CanWander.Remove(playerId);
            return;
        }

        RouteEnds[playerId] = new Vector2(x, y);
        if (rooms.Count >= OptionGroupSingleton<NomadOptions>.Instance.RoomsPerRoute)
            CanWander.Add(playerId);
    }

    [MethodRpc((uint)CustomRPC.NomadRequestWander)]
    public static void RpcRequestWander(PlayerControl source)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Nomad || source.Data.IsDead || !CanWander.Contains(source.PlayerId) || !Anchors.TryGetValue(source.PlayerId, out var anchor))
            return;

        RpcConfirmWander(PlayerControl.LocalPlayer, source.PlayerId, anchor.x, anchor.y);
    }

    [MethodRpc((uint)CustomRPC.NomadConfirmWander, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmWander(PlayerControl host, byte playerId, float x, float y)
    {
        if (!host.IsHost() || !CanWander.Remove(playerId))
            return;

        Scores.TryGetValue(playerId, out var score);
        Scores[playerId] = score + 1;
        if (RouteEnds.TryGetValue(playerId, out var routeEnd))
            BacktrackPositions[playerId] = routeEnd;

        Anchors.Remove(playerId);
        LastRooms.Remove(playerId);
        RouteRooms.Remove(playerId);
        RouteEnds.Remove(playerId);

        var player = Utils.PlayerById(playerId);
        if (player.AmOwner)
            player.NetTransform.RpcSnapTo(new Vector2(x, y));
    }

    [MethodRpc((uint)CustomRPC.NomadConfirmBacktrack, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmBacktrack(PlayerControl host, byte playerId, float x, float y)
    {
        if (!host.IsHost() || !BacktrackPositions.Remove(playerId))
            return;

        var player = Utils.PlayerById(playerId);
        if (!player.AmOwner)
            return;

        player.NetTransform.RpcSnapTo(new Vector2(x, y));
        Coroutines.Start(CoroutinesHelper.CoNotify("<color=#78E8FF>Backtrack:</color> your completed route pulled you out of danger."));
    }
}