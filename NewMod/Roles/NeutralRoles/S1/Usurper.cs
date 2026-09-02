using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Buttons.Roles.S1;
using NewMod.Options.Roles.S1;
using NewMod.RoleLogic;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles.S1;

[MiraIgnore]
public sealed class Usurper : CrewmateRole, INewModRole
{
    public static readonly Dictionary<byte, UsurperCrownState> States = [];
    public static readonly Dictionary<byte, Vector2> CrownPositions = [];

    private static readonly Dictionary<byte, GameObject> CrownObjects = [];

    public static byte ExiledPlayerId = byte.MaxValue;
    public static Vector2 ExilePosition;

    public string RoleName => "Usurper";
    public string RoleDescription => "Secretly claim a player, take their Crown after death, then survive a meeting.";
    public string RoleLongDescription => "Claim one player in secret. Their death leaves a visible Crown. Reach it first, take it without revealing your identity, and survive until the next meeting ends.";
    public Color RoleColor => new(0.72f, 0.34f, 0.16f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public NewModFaction Faction => NewModFaction.Entropy;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            MaxRoleCount = 1,
            DefaultRoleCount = 1,
            DefaultChance = 30,
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
        var state = States[PlayerControl.LocalPlayer.PlayerId];

        if (state.Phase == UsurperCrownPhase.Claimed)
            text.AppendLine($"<size=65%>Claimed: <color=#D8844D>{Utils.PlayerById(state.TargetId).Data.PlayerName}</color></size>");
        else if (state.Phase == UsurperCrownPhase.Available)
            text.AppendLine("<size=65%><color=#F0B26E>The Crown is waiting.</color></size>");
        else if (state.Phase == UsurperCrownPhase.Held)
            text.AppendLine("<size=65%><color=#75E6A5>Survive the next meeting.</color></size>");

        return text;
    }

    public override bool DidWin(GameOverReason reason)
    {
        return reason == CustomGameOver.GameOverReason<UsurperGameOver>();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            foreach (var crown in CrownObjects.Values)
                Object.Destroy(crown);

            States.Clear();
            CrownPositions.Clear();
            CrownObjects.Clear();
            ExiledPlayerId = byte.MaxValue;
            ExilePosition = Vector2.zero;

            foreach (var player in PlayerControl.AllPlayerControls)
                if (player.Data.Role is Usurper)
                    States[player.PlayerId] = new UsurperCrownState();

            return;
        }

        if (!AmongUsClient.Instance.AmHost || ExiledPlayerId == byte.MaxValue)
            return;

        MarkClaimedDeath(ExiledPlayerId, ExilePosition);
        ExiledPlayerId = byte.MaxValue;
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        var position = evt.DeadBody ? (Vector2)evt.DeadBody.transform.position : evt.Target.GetTruePosition();
        MarkClaimedDeath(evt.Target.PlayerId, position);
    }

    [RegisterEvent]
    public static void OnMeetingEnd(EndMeetingEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        foreach (var pair in States)
        {
            var usurper = Utils.PlayerById(pair.Key);
            if (!pair.Value.SurvivedMeeting(!usurper.Data.IsDead && !usurper.Data.Disconnected))
                continue;

            CustomGameOver.Trigger<UsurperGameOver>([usurper.Data]);
            return;
        }
    }

    [RegisterEvent]
    public static void OnPlayerLeave(PlayerLeaveEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost || evt.ClientData.Character == null)
            return;

        var targetId = evt.ClientData.Character.PlayerId;
        foreach (var pair in States)
            if (pair.Value.Phase == UsurperCrownPhase.Claimed && pair.Value.TargetId == targetId)
                RpcRefundClaim(PlayerControl.LocalPlayer, pair.Key, targetId);
    }

    public static void HostFixedUpdate()
    {
        var range = OptionGroupSingleton<UsurperOptions>.Instance.CrownPickupRange;
        foreach (var pair in CrownPositions.ToArray())
        {
            var usurper = Utils.PlayerById(pair.Key);
            if (!usurper.Data.IsDead && !usurper.Data.Disconnected && Vector2.Distance(usurper.GetTruePosition(), pair.Value) <= range)
                RpcTakeCrown(PlayerControl.LocalPlayer, pair.Key);
        }
    }

    public static void MarkClaimedDeath(byte targetId, Vector2 position)
    {
        foreach (var pair in States)
            if (pair.Value.Phase == UsurperCrownPhase.Claimed && pair.Value.TargetId == targetId)
                RpcSpawnCrown(PlayerControl.LocalPlayer, pair.Key, targetId, position.x, position.y);
    }

    [MethodRpc((uint)CustomRPC.UsurperRequestClaim)]
    public static void RpcRequestClaim(PlayerControl source, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Usurper || source.Data.IsDead || target.Data.IsDead || target.Data.Disconnected || !States.TryGetValue(source.PlayerId, out var state) || state.Phase != UsurperCrownPhase.Unclaimed || Vector2.Distance(source.GetTruePosition(), target.GetTruePosition()) > OptionGroupSingleton<UsurperOptions>.Instance.ClaimRange)
            return;

        RpcConfirmClaim(PlayerControl.LocalPlayer, source.PlayerId, target.PlayerId);
    }

    [MethodRpc((uint)CustomRPC.UsurperConfirmClaim, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmClaim(PlayerControl host, byte usurperId, byte targetId)
    {
        if (host.IsHost())
            States[usurperId].Claim(targetId);
    }

    [MethodRpc((uint)CustomRPC.UsurperSpawnCrown, LocalHandling = RpcLocalHandling.After)]
    public static void RpcSpawnCrown(PlayerControl host, byte usurperId, byte targetId, float x, float y)
    {
        if (!host.IsHost() || !States[usurperId].MakeCrownAvailable(targetId))
            return;

        var position = new Vector2(x, y);
        CrownPositions[usurperId] = position;

        var crown = new GameObject($"UsurperCrown_{usurperId}");
        crown.transform.SetParent(ShipStatus.Instance.transform, true);
        crown.transform.position = new Vector3(x, y, -1f);
        crown.transform.localScale = Vector3.one * 0.35f;

        var renderer = crown.AddComponent<SpriteRenderer>();
        renderer.sprite = NewModAsset.CrownIcon.LoadAsset();
        renderer.color = new Color(1f, 0.78f, 0.2f);
        CrownObjects[usurperId] = crown;
    }

    [MethodRpc((uint)CustomRPC.UsurperTakeCrown, LocalHandling = RpcLocalHandling.After)]
    public static void RpcTakeCrown(PlayerControl host, byte usurperId)
    {
        if (!host.IsHost() || !States[usurperId].TakeCrown())
            return;

        CrownPositions.Remove(usurperId);
        Object.Destroy(CrownObjects[usurperId]);
        CrownObjects.Remove(usurperId);

        Coroutines.Start(CoroutinesHelper.CoNotify("<color=#F0B26E>The Crown has been claimed.</color>"));
        if (PlayerControl.LocalPlayer.PlayerId == usurperId)
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#75E6A5>Hold the Crown:</color> survive the next meeting."));
    }

    [MethodRpc((uint)CustomRPC.UsurperRefundClaim, LocalHandling = RpcLocalHandling.After)]
    public static void RpcRefundClaim(PlayerControl host, byte usurperId, byte targetId)
    {
        if (!host.IsHost() || !States[usurperId].Refund(targetId))
            return;

        var usurper = Utils.PlayerById(usurperId);
        if (usurper.AmOwner)
        {
            CustomButtonSingleton<ClaimButton>.Instance.SetUses(1);
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#D8844D>Claim refunded:</color> your target disconnected."));
        }
    }
}

