using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Translation;
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
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewMod.Roles.NeutralRoles.S1;

[MiraIgnore]
public class Bounty : CrewmateRole, INewModRole
{
    public static uint NextContractId;
    public static readonly Dictionary<byte, (uint Id, byte TargetId, float Progress, float AwayTime, ContractPhase Phase)> Contracts = [];
    public static readonly Dictionary<byte, float> CollectionExpiresAt = [];
    public static readonly Dictionary<byte, float> CollectionStartsAt = [];
    public static readonly Dictionary<byte, byte> PendingCashOut = [];

    public static GameObject _collectionArrow;

    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Bounty");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Bounty.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Bounty.TabDescription");
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
            text.AppendLine(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Bounty.Tab.Waiting"));
            return text;
        }

        var target = Utils.PlayerById(contract.TargetId);
        text.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Bounty.Tab.Contract"), target.Data.PlayerName));
        text.AppendLine(contract.Phase == ContractPhase.Collection ? string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Bounty.Tab.CollectionCountdown"), Mathf.Max(0, Mathf.CeilToInt(CollectionExpiresAt[PlayerControl.LocalPlayer.PlayerId] - Time.time))) : MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.Bounty.Tab.EscortInstructions"));
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
        CollectionStartsAt.Clear();
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
        CollectionStartsAt.Remove(evt.Player.PlayerId);
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
            if (pair.Value.Phase == ContractPhase.Collection)
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
            if (killedByBounty)
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
            if (!bounty || bounty.Data.IsDead || bounty.Data.Disconnected)
                continue;

            if (!target || target.Data.IsDead || target.Data.Disconnected)
            {
                RpcFailContract(PlayerControl.LocalPlayer, pair.Key);
                AssignContract(pair.Key);
                continue;
            }

            if (pair.Value.Phase == ContractPhase.Collection)
            {
                if (Time.time >= CollectionExpiresAt[pair.Key])
                {
                    RpcFailContract(PlayerControl.LocalPlayer, pair.Key);
                    AssignContract(pair.Key);
                }

                continue;
            }

            if (Vector2.Distance(bounty.GetTruePosition(), target.GetTruePosition()) <= options.EscortRange && !PhysicsHelpers.AnythingBetween(bounty.GetTruePosition(), target.GetTruePosition(), Constants.ShipAndObjectsMask, false))
            {
                if (AdvanceContract(pair.Key, Time.fixedDeltaTime, options.EscortDuration))
                    RpcBeginCollection(PlayerControl.LocalPlayer, pair.Key, options.CollectionDuration);
            }
            else
            {
                LoseContact(pair.Key, Time.fixedDeltaTime, options.ContactGrace);
            }
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
    public static void RpcAssignContract(PlayerControl source, byte bountyId, byte targetId)
    {
        if (!source.IsHost())
            return;

        Contracts[bountyId] = (++NextContractId, targetId, 0f, 0f, ContractPhase.Escort);
        CollectionExpiresAt.Remove(bountyId);
        CollectionStartsAt.Remove(bountyId);
        PendingCashOut.Remove(bountyId);

        if (PlayerControl.LocalPlayer.PlayerId == bountyId)
        {
            PlayerControl.LocalPlayer.CancelPlayerTracking();
            Coroutines.Start(CoTrackContract(Contracts[bountyId].Id));
        }
    }

    [MethodRpc((uint)CustomRPC.BountyBeginCollection, LocalHandling = RpcLocalHandling.After)]
    public static void RpcBeginCollection(PlayerControl source, byte bountyId, float duration)
    {
        if (!source.IsHost() || !Contracts.TryGetValue(bountyId, out var contract))
            return;

        contract.Phase = ContractPhase.Collection;
        Contracts[bountyId] = contract;
        CollectionStartsAt[bountyId] = Time.time + OptionGroupSingleton<BountyOptions>.Instance.HeadStart;
        CollectionExpiresAt[bountyId] = CollectionStartsAt[bountyId] + duration;
        var local = PlayerControl.LocalPlayer;

        if (local.PlayerId == bountyId)
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FFB14F>Contract ready:</color> give your target a head start,\nthen catch them before time runs out."));

        if (local.PlayerId != contract.TargetId)
            return;

        ShowCollectionArrow(Utils.PlayerById(bountyId));
        Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FFB14F>Collection:</color> Your Bounty is coming for you."));
    }

    [MethodRpc((uint)CustomRPC.BountyFailContract, LocalHandling = RpcLocalHandling.After)]
    public static void RpcFailContract(PlayerControl source, byte bountyId)
    {
        if (!source.IsHost() || !Contracts.Remove(bountyId, out var contract))
            return;

        CollectionExpiresAt.Remove(bountyId);
        CollectionStartsAt.Remove(bountyId);
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
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Bounty || source.Data.IsDead || !Contracts.TryGetValue(source.PlayerId, out var contract) || MeetingHud.Instance || ExileController.Instance || !CanCollect(source.PlayerId, Time.time))
            return;

        var target = Utils.PlayerById(contract.TargetId);
        if (target.Data.IsDead || target.Data.Disconnected || target.inVent || PhysicsHelpers.AnythingBetween(source.GetTruePosition(), target.GetTruePosition(), Constants.ShipAndObjectsMask, false) || Vector2.Distance(source.GetTruePosition(), target.GetTruePosition()) > OptionGroupSingleton<BountyOptions>.Instance.CashOutRange)
            return;

        PendingCashOut[source.PlayerId] = target.PlayerId;
        source.RpcCustomMurder(target, true, false, true, false);
    }

    public static IEnumerator CoTrackContract(uint contractId)
    {
        var local = PlayerControl.LocalPlayer;
        while (!local.Data.IsDead && Contracts.TryGetValue(local.PlayerId, out var current) && current.Id == contractId)
        {
            var target = Utils.PlayerById(current.TargetId);
            if (!target || target.Data.IsDead || target.Data.Disconnected) break;
            if (!MeetingHud.Instance && !ExileController.Instance)
                local.StartPlayerTracking(target, target.Data.DefaultOutfit.ColorId);
            yield return new WaitForSeconds(0.5f);
            if (!Contracts.TryGetValue(local.PlayerId, out current) || current.Id != contractId) yield break;
            local.CancelPlayerTracking();
            yield return new WaitForSeconds(OptionGroupSingleton<BountyOptions>.Instance.TrackingInterval);
        }

        local.CancelPlayerTracking();
    }

    public static void ShowCollectionArrow(PlayerControl bounty)
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

    public static IEnumerator CoFollowBounty(ArrowBehaviour arrow, PlayerControl bounty)
    {
        while (_collectionArrow && !bounty.Data.Disconnected)
        {
            arrow.target = bounty.transform.position;
            yield return null;
        }
    }

    public static void HideCollectionArrow()
    {
        if (_collectionArrow)
            Destroy(_collectionArrow);
    }

    public enum ContractPhase : byte
    {
        Escort,
        Collection
    }

    public static bool AdvanceContract(byte ownerId, float seconds, float required)
    {
        var contract = Contracts[ownerId];
        if (contract.Phase != ContractPhase.Escort)
            return false;

        contract.AwayTime = 0f;
        contract.Progress = Math.Min(required, contract.Progress + seconds);
        if (contract.Progress >= required)
            contract.Phase = ContractPhase.Collection;
        Contracts[ownerId] = contract;
        return contract.Phase == ContractPhase.Collection;
    }

    public static void LoseContact(byte ownerId, float seconds, float grace)
    {
        var contract = Contracts[ownerId];
        if (contract.Phase != ContractPhase.Escort)
            return;

        var previous = contract.AwayTime;
        contract.AwayTime += seconds;
        var lost = Math.Max(0f, contract.AwayTime - grace) - Math.Max(0f, previous - grace);
        contract.Progress = Math.Max(0f, contract.Progress - lost);
        Contracts[ownerId] = contract;
    }

    public static bool CanCollect(byte ownerId, float now)
    {
        return Contracts.TryGetValue(ownerId, out var contract) && contract.Phase == ContractPhase.Collection
            && now >= CollectionStartsAt.GetValueOrDefault(ownerId) && now < CollectionExpiresAt.GetValueOrDefault(ownerId);
    }
}