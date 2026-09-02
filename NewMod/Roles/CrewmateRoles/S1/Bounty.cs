using System.Collections;
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
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.RoleLogic;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewMod.Roles.NeutralRoles.S1;

[MiraIgnore]
public sealed class Bounty : CrewmateRole, INewModRole
{
    public static readonly Dictionary<byte, BountyContractState> Contracts = [];
    public static readonly Dictionary<byte, float> CollectionExpiresAt = [];
    public static readonly Dictionary<byte, byte> PendingCashOut = [];

    private static GameObject _collectionArrow;

    public string RoleName => "Bounty";
    public string RoleDescription => "Stay close to your contract, then collect before time runs out.";
    public string RoleLongDescription => "Escort your target to fill the contract. Collection warns both players and gives the target your direction. Cash Out before the window closes.";
    public Color RoleColor => new(0.96f, 0.58f, 0.16f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public NewModFaction Faction => NewModFaction.Apex;

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
        if (!Contracts.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var contract))
        {
            text.AppendLine("<size=65%><color=#A4A4A4>Waiting for a contract.</color></size>");
            return text;
        }

        var target = Utils.PlayerById(contract.TargetId);
        text.AppendLine($"<size=65%>Contract: <color=#FFB14F>{target.Data.PlayerName}</color></size>");
        text.AppendLine(contract.Phase == BountyPhase.Collection
            ? $"<size=65%><color=#FFCF70>COLLECTION:</color> {Mathf.Max(0, Mathf.CeilToInt(CollectionExpiresAt[PlayerControl.LocalPlayer.PlayerId] - Time.time))}s</size>"
            : "<size=65%>Stay close to advance the contract.</size>");
        return text;
    }

    public override bool DidWin(GameOverReason reason)
    {
        return reason == CustomGameOver.GameOverReason<BountyGameOver>();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
            return;

        Contracts.Clear();
        CollectionExpiresAt.Clear();
        PendingCashOut.Clear();
        HideCollectionArrow();

        if (PlayerControl.LocalPlayer.Data.Role is Bounty)
            PlayerControl.LocalPlayer.CancelPlayerTracking();

        if (!AmongUsClient.Instance.AmHost)
            return;

        foreach (var player in PlayerControl.AllPlayerControls)
            if (player.Data.Role is Bounty)
                AssignContract(player.PlayerId);
    }

    [RegisterEvent]
    public static void OnSetRole(SetRoleEvent evt)
    {
        if (evt.Player.Data.Role is Bounty)
        {
            if (AmongUsClient.Instance.AmHost && !Contracts.ContainsKey(evt.Player.PlayerId))
                AssignContract(evt.Player.PlayerId);
            return;
        }

        if (!Contracts.Remove(evt.Player.PlayerId))
            return;

        CollectionExpiresAt.Remove(evt.Player.PlayerId);
        PendingCashOut.Remove(evt.Player.PlayerId);
        if (evt.Player.AmOwner)
            evt.Player.CancelPlayerTracking();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        foreach (var pair in Contracts.ToArray())
            if (pair.Value.Phase == BountyPhase.Collection)
                RpcFailContract(PlayerControl.LocalPlayer, pair.Key);
    }

    [RegisterEvent]
    public static void OnMeetingEnd(EndMeetingEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        foreach (var player in PlayerControl.AllPlayerControls)
            if (player.Data.Role is Bounty && !player.Data.IsDead && !player.Data.Disconnected && !Contracts.ContainsKey(player.PlayerId))
                AssignContract(player.PlayerId);
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        foreach (var pair in Contracts.ToArray())
        {
            if (pair.Value.TargetId != evt.Target.PlayerId)
                continue;

            var killedByBounty = PendingCashOut.TryGetValue(pair.Key, out var targetId) && targetId == evt.Target.PlayerId && evt.Source.PlayerId == pair.Key;
            if (BountyContractState.ResolveCollection(killedByBounty, true, false) == BountyResolution.Win)
            {
                CustomGameOver.Trigger<BountyGameOver>([evt.Source.Data]);
                return;
            }

            RpcFailContract(PlayerControl.LocalPlayer, pair.Key);
            AssignContract(pair.Key);
        }
    }

    [RegisterEvent]
    public static void OnPlayerLeave(PlayerLeaveEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost || evt.ClientData.Character == null)
            return;

        var departedId = evt.ClientData.Character.PlayerId;
        foreach (var pair in Contracts.ToArray())
        {
            if (pair.Value.TargetId != departedId)
                continue;

            RpcFailContract(PlayerControl.LocalPlayer, pair.Key);
            AssignContract(pair.Key);
        }
    }

    public static void HostFixedUpdate()
    {
        var options = OptionGroupSingleton<BountyOptions>.Instance;
        foreach (var pair in Contracts.ToArray())
        {
            var bounty = Utils.PlayerById(pair.Key);
            var target = Utils.PlayerById(pair.Value.TargetId);
            if (bounty.Data.IsDead || bounty.Data.Disconnected)
                continue;

            if (target.Data.IsDead || target.Data.Disconnected)
            {
                RpcFailContract(PlayerControl.LocalPlayer, pair.Key);
                AssignContract(pair.Key);
                continue;
            }

            if (pair.Value.Phase == BountyPhase.Collection)
            {
                if (Time.time >= CollectionExpiresAt[pair.Key])
                {
                    RpcFailContract(PlayerControl.LocalPlayer, pair.Key);
                    AssignContract(pair.Key);
                }
                continue;
            }

            if (Vector2.Distance(bounty.GetTruePosition(), target.GetTruePosition()) <= options.EscortRange && pair.Value.Advance(Time.fixedDeltaTime, options.EscortDuration))
                RpcBeginCollection(PlayerControl.LocalPlayer, pair.Key, options.CollectionDuration);
        }
    }

    public static void AssignContract(byte bountyId)
    {
        var candidates = PlayerControl.AllPlayerControls.ToArray().Where(player => player.PlayerId != bountyId && !player.Data.IsDead && !player.Data.Disconnected && player.Data.Role is not Bounty).ToArray();
        if (candidates.Length == 0)
            return;

        RpcAssignContract(PlayerControl.LocalPlayer, bountyId, candidates[Random.Range(0, candidates.Length)].PlayerId);
    }

    [MethodRpc((uint)CustomRPC.BountyAssignContract, LocalHandling = RpcLocalHandling.After)]
    public static void RpcAssignContract(PlayerControl host, byte bountyId, byte targetId)
    {
        if (!host.IsHost())
            return;

        Contracts[bountyId] = new BountyContractState(targetId);
        CollectionExpiresAt.Remove(bountyId);
        PendingCashOut.Remove(bountyId);

        if (PlayerControl.LocalPlayer.PlayerId == bountyId)
        {
            var target = Utils.PlayerById(targetId);
            PlayerControl.LocalPlayer.CancelPlayerTracking();
            PlayerControl.LocalPlayer.StartPlayerTracking(target, target.Data.DefaultOutfit.ColorId);
        }
    }

    [MethodRpc((uint)CustomRPC.BountyBeginCollection, LocalHandling = RpcLocalHandling.After)]
    public static void RpcBeginCollection(PlayerControl host, byte bountyId, float duration)
    {
        if (!host.IsHost() || !Contracts.TryGetValue(bountyId, out var contract))
            return;

        contract.BeginCollection();
        CollectionExpiresAt[bountyId] = Time.time + duration;
        var local = PlayerControl.LocalPlayer;

        if (local.PlayerId == bountyId)
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FFB14F>Collection open:</color> Cash out before time expires."));

        if (local.PlayerId != contract.TargetId)
            return;

        ShowCollectionArrow(Utils.PlayerById(bountyId));
        Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FFB14F>Collection:</color> Your Bounty is coming for you."));
    }

    [MethodRpc((uint)CustomRPC.BountyFailContract, LocalHandling = RpcLocalHandling.After)]
    public static void RpcFailContract(PlayerControl host, byte bountyId)
    {
        if (!host.IsHost() || !Contracts.Remove(bountyId, out var contract))
            return;

        CollectionExpiresAt.Remove(bountyId);
        PendingCashOut.Remove(bountyId);
        var local = PlayerControl.LocalPlayer;

        if (local.PlayerId == bountyId)
        {
            local.CancelPlayerTracking();
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FFB14F>Contract failed.</color> A new target will be assigned."));
        }

        if (local.PlayerId == contract.TargetId)
            HideCollectionArrow();
    }

    [MethodRpc((uint)CustomRPC.BountyRequestCashOut)]
    public static void RpcRequestCashOut(PlayerControl source)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Bounty || source.Data.IsDead || !Contracts.TryGetValue(source.PlayerId, out var contract) || contract.Phase != BountyPhase.Collection || Time.time >= CollectionExpiresAt[source.PlayerId])
            return;

        var target = Utils.PlayerById(contract.TargetId);
        if (target.Data.IsDead || target.Data.Disconnected || Vector2.Distance(source.GetTruePosition(), target.GetTruePosition()) > OptionGroupSingleton<BountyOptions>.Instance.CashOutRange)
            return;

        PendingCashOut[source.PlayerId] = target.PlayerId;
        source.RpcCustomMurder(target, true, false, true, false);
    }

    private static void ShowCollectionArrow(PlayerControl bounty)
    {
        HideCollectionArrow();
        _collectionArrow = new GameObject("BountyCollectionArrow") { layer = 5 };
        var renderer = _collectionArrow.AddComponent<SpriteRenderer>();
        renderer.sprite = NewModAsset.Arrow.LoadAsset();
        renderer.color = new Color(0.96f, 0.58f, 0.16f);
        var arrow = _collectionArrow.AddComponent<ArrowBehaviour>();
        arrow.alwaysMaxSize = true;
        arrow.MaxScale = 0.65f;
        Coroutines.Start(CoFollowBounty(arrow, bounty));
    }

    private static IEnumerator CoFollowBounty(ArrowBehaviour arrow, PlayerControl bounty)
    {
        while (_collectionArrow && !bounty.Data.Disconnected)
        {
            arrow.target = bounty.transform.position;
            yield return null;
        }
    }

    private static void HideCollectionArrow()
    {
        if (_collectionArrow)
            Object.Destroy(_collectionArrow);
    }
}
