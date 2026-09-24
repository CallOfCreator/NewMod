using System.Collections;
using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Translation;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles;
using NewMod.RoleLogic;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles;

public class EgoistRole : CrewmateRole, INewModRole
{
    public static readonly Dictionary<byte, EgoistChallengeState> States = [];

    public static bool _selecting;
    public static byte PendingWinner = byte.MaxValue;

    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.EgoistRole");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.EgoistRole.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.EgoistRole.TabDescription");
    public Color RoleColor => new(0.8f, 0.3f, 0.6f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleOptionsGroup RoleOptionsGroup => RoleOptionsGroup.Neutral;
    public NewModFaction Faction => NewModFaction.Entropy;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            AffectedByLightOnAirship = false,
            CanGetKilled = true,
            UseVanillaKillButton = false,
            CanUseVent = false,
            CanUseSabotage = false,
            TasksCountForProgress = false,
            ShowInFreeplay = true,
            HideSettings = false,
            MaxRoleCount = 1,
            OptionsScreenshot = MiraAssets.Empty,
            Icon = MiraAssets.Empty
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = INewModRole.GetRoleTabText(this);
        var state = States[PlayerControl.LocalPlayer.PlayerId];
        var required = (int)OptionGroupSingleton<EgoistRoleOptions>.Instance.EgoRequired;
        text.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.EgoistRole.Tab.Ego"), state.Ego, required));
        if (state.Active)
            text.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.EgoistRole.Tab.Opponent"), Utils.PlayerById(state.OpponentId).Data.PlayerName));
        return text;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<EgoistGameOver>();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro)
        {
            if (AmongUsClient.Instance.AmHost && GameManager.Instance.ShouldCheckForGameEnd && PendingWinner != byte.MaxValue)
            {
                var winner = Utils.PlayerById(PendingWinner);
                PendingWinner = byte.MaxValue;
                if (winner && !winner.Data.IsDead && !winner.Data.Disconnected && winner.Data.Role is EgoistRole)
                    CustomGameOver.Trigger<EgoistGameOver>([winner.Data]);
            }

            return;
        }

        PendingWinner = byte.MaxValue;

        States.Clear();
        _selecting = false;
        foreach (var player in PlayerControl.AllPlayerControls)
            if (player.Data.Role is EgoistRole)
                States[player.PlayerId] = new EgoistChallengeState();
    }

    [RegisterEvent]
    public static void OnSetRole(SetRoleEvent evt)
    {
        if (evt.Player.Data.Role is EgoistRole) States[evt.Player.PlayerId] = new EgoistChallengeState();
        else States.Remove(evt.Player.PlayerId);
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        _selecting = false;
        var local = PlayerControl.LocalPlayer;
        if (local.Data.IsDead || local.Data.Role is not EgoistRole)
            return;

        Coroutines.Start(CoSetupMeetingButton(evt.MeetingHud));
    }

    [RegisterEvent]
    public static void OnMeetingSelect(MeetingSelectEvent evt)
    {
        var local = PlayerControl.LocalPlayer;
        if (_selecting && !local.Data.IsDead && local.Data.Role is EgoistRole && !States[local.PlayerId].Active)
        {
            if (evt.TargetId < 0)
                return;

            if (evt.TargetId == local.PlayerId)
            {
                evt.AllowSelect = false;
                return;
            }

            var target = Utils.PlayerById((byte)evt.TargetId);
            if (target.Data.IsDead || target.Data.Disconnected)
                return;

            evt.AllowSelect = false;
            _selecting = false;
            RpcRequestChallenge(local, target);
            return;
        }
    }

    [RegisterEvent]
    public static void OnProcessVotes(ProcessVotesEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;

        foreach (var pair in States)
        {
            var egoist = Utils.PlayerById(pair.Key);
            if (egoist.Data.IsDead || egoist.Data.Disconnected)
                continue;

            if (pair.Value.Active)
            {
                var exiledId = evt.ExiledPlayer?.PlayerId ?? byte.MaxValue;
                var result = pair.Value.Resolve(exiledId, pair.Key);
                RpcResolveChallenge(PlayerControl.LocalPlayer, pair.Key, exiledId);

                if (result == EgoistChallengeResult.Win)
                    PendingWinner = pair.Key;
                continue;
            }

            if (evt.ExiledPlayer?.PlayerId == pair.Key)
                continue;

            var votes = 0;
            foreach (var vote in evt.Votes)
                if (vote.Suspect == pair.Key && vote.Voter != pair.Key)
                    votes++;

            if (votes == 0)
                continue;

            pair.Value.AddVotes(votes, (int)OptionGroupSingleton<EgoistRoleOptions>.Instance.EgoRequired);
            RpcSetEgo(PlayerControl.LocalPlayer, pair.Key, pair.Value.Ego);
        }
    }

    public static void OnMeetingAbilityClicked()
    {
        var local = PlayerControl.LocalPlayer;
        var state = States[local.PlayerId];
        if (state.Active || state.Ego < OptionGroupSingleton<EgoistRoleOptions>.Instance.EgoRequired)
            return;

        _selecting = true;
        UpdateMeetingButton();
        Coroutines.Start(CoroutinesHelper.CoNotify("<color=#CC4D99>Challenge:</color> select your opponent."));
    }

    [MethodRpc((uint)CustomRPC.EgoistRequestChallenge)]
    public static void RpcRequestChallenge(PlayerControl source, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost || !MeetingHud.Instance || MeetingHud.Instance.CurrentState is MeetingHud.MeetingStates.Results or MeetingHud.MeetingStates.Proceeding || source.Data.Role is not EgoistRole || source.Data.IsDead || target.Data.IsDead || target.Data.Disconnected || source == target || !States.TryGetValue(source.PlayerId, out var state) || state.Active || state.Ego < OptionGroupSingleton<EgoistRoleOptions>.Instance.EgoRequired)
            return;

        RpcConfirmChallenge(PlayerControl.LocalPlayer, source.PlayerId, target.PlayerId);
    }

    [MethodRpc((uint)CustomRPC.EgoistConfirmChallenge, LocalHandling = RpcLocalHandling.After)]
    public static void RpcConfirmChallenge(PlayerControl source, byte egoistId, byte opponentId)
    {
        if (!source.IsHost() || !States[egoistId].Start(opponentId))
            return;

        var egoist = Utils.PlayerById(egoistId);
        var opponent = Utils.PlayerById(opponentId);
        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#CC4D99>Public challenge:</color> {egoist.Data.PlayerName} versus {opponent.Data.PlayerName}."));
        if (egoist.AmOwner)
            UpdateMeetingButton();
    }

    [MethodRpc((uint)CustomRPC.EgoistSetEgo, LocalHandling = RpcLocalHandling.After)]
    public static void RpcSetEgo(PlayerControl source, byte egoistId, int ego)
    {
        if (source.IsHost())
            States[egoistId].SetEgo(ego);
    }

    [MethodRpc((uint)CustomRPC.EgoistResolveChallenge, LocalHandling = RpcLocalHandling.After)]
    public static void RpcResolveChallenge(PlayerControl source, byte egoistId, byte exiledId)
    {
        if (!source.IsHost())
            return;

        if (!AmongUsClient.Instance.AmHost)
            States[egoistId].Resolve(exiledId, egoistId);
    }

    public static void UpdateMeetingButton()
    {
        if (!MeetingHud.Instance || PlayerControl.LocalPlayer.Data.Role is not EgoistRole)
            return;

        var state = States[PlayerControl.LocalPlayer.PlayerId];
        var button = MeetingHud.Instance.MeetingAbilityButton;
        if (state.Ego < OptionGroupSingleton<EgoistRoleOptions>.Instance.EgoRequired && !state.Active)
        {
            button.Hide();
            return;
        }

        button.Show();
        button.SetInfiniteUses();
        button.SetCoolDown(0f, 1f);
        button.graphic.sprite = MiraAssets.Empty.LoadAsset();
        button.graphic.SetCooldownNormalizedUvs();
        button.OverrideColor(new Color32(204, 77, 153, 255));
        button.OverrideText(state.Active ? "LOCKED" : _selecting ? "SELECT" : "CHALLENGE");
    }

    public static IEnumerator CoSetupMeetingButton(MeetingHud hud)
    {
        while (hud && hud.CurrentState == MeetingHud.MeetingStates.Animating)
            yield return null;

        yield return new WaitForSeconds(0.25f);
        if (hud && PlayerControl.LocalPlayer.Data.Role is EgoistRole)
            UpdateMeetingButton();
    }
}