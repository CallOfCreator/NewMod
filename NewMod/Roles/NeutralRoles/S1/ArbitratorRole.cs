using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using NewMod.Options.Roles.S1;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles.S1;

public enum ArbitratorJudgmentMode : byte
{
    Accuse,
    Defend,
}

[MiraIgnore]
public class ArbitratorRole : CrewmateRole, INewModRole
{
    public static readonly Dictionary<byte, byte> JudgmentTokens = [];
    public static readonly Dictionary<byte, byte> LastVotes = [];
    public static readonly HashSet<byte> ScoredTargets = [];

    public static byte PendingWinner = byte.MaxValue;
    public static ArbitratorJudgmentMode _localMode;
    public static bool _localLocked;

    public static byte _hostOwner = byte.MaxValue;
    public static byte _hostTarget = byte.MaxValue;
    public static ArbitratorJudgmentMode _hostMode;
    public static bool _judgmentResolved;

    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.ArbitratorRole");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.ArbitratorRole.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.ArbitratorRole.TabDescription");

    public Color RoleColor => new Color32(215, 176, 82, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleOptionsGroup RoleOptionsGroup => RoleOptionsGroup.Neutral;
    public NewModFaction Faction => NewModFaction.Entropy;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            OptionsScreenshot = MiraAssets.Empty,
            Icon = MiraAssets.Empty,
            CanGetKilled = true,
            UseVanillaKillButton = false,
            CanUseVent = false,
            CanUseSabotage = false,
            TasksCountForProgress = false,
            MaxRoleCount = 1,
            DefaultChance = 25,
            DefaultRoleCount = 1,
            CanModifyChance = true,
            ShowInFreeplay = true,
            GhostRole = RoleTypes.Crewmate,
            RoleHintType = RoleHintType.RoleTab,
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tabText = INewModRole.GetRoleTabText(this);
        JudgmentTokens.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var tokens);

        var required = (int)OptionGroupSingleton<ArbitratorOptions>.Instance.JudgmentTokensToWin;
        var defendVotes = (int)OptionGroupSingleton<ArbitratorOptions>.Instance.DefendVotesRequired;

        tabText.AppendLine();
        tabText.AppendLine(string.Format(CultureInfo.CurrentCulture, MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.ArbitratorRole.Tab.JudgmentTokens"), tokens, required));

        tabText.AppendLine(string.Format(CultureInfo.CurrentCulture, MiraLocaleManager.Get("NewMod.Roles.NeutralRoles.S1.ArbitratorRole.Tab.Defend"), defendVotes));

        return tabText;
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            ResetState();
        }
        else if (AmongUsClient.Instance.AmHost && GameManager.Instance.ShouldCheckForGameEnd && PendingWinner != byte.MaxValue)
        {
            var winner = Utils.PlayerById(PendingWinner);
            PendingWinner = byte.MaxValue;
            if (winner && !winner.Data.IsDead && !winner.Data.Disconnected && winner.Data.Role is ArbitratorRole)
                CustomGameOver.Trigger<ArbitratorGameOver>([winner.Data]);
        }
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        ResetState();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        _localMode = ArbitratorJudgmentMode.Accuse;
        _localLocked = false;
        _judgmentResolved = false;

        if (AmongUsClient.Instance.AmHost)
        {
            _hostOwner = byte.MaxValue;
            _hostTarget = byte.MaxValue;
        }

        var player = PlayerControl.LocalPlayer;

        if (player.Data.IsDead || player.Data.Role is not ArbitratorRole)
            return;

        Coroutines.Start(CoSetupMeetingButton(evt.MeetingHud));
    }

    [RegisterEvent]
    public static void OnMeetingSelect(MeetingSelectEvent evt)
    {
        if (_localLocked || PlayerControl.LocalPlayer.Data.IsDead || PlayerControl.LocalPlayer.Data.Role is not ArbitratorRole || !IsJudgmentOpen())
            return;

        evt.AllowSelect = false;
        var target = Utils.PlayerById((byte)evt.TargetId);

        if (!target || target == PlayerControl.LocalPlayer || target.Data.IsDead || target.Data.Disconnected || ScoredTargets.Contains(target.PlayerId))
        {
            Coroutines.Start(CoroutinesHelper.CoNotify("Choose another living player.\nEach player can only score once."));
            return;
        }

        _localLocked = true;

        RpcSetJudgment(PlayerControl.LocalPlayer, (byte)_localMode, target.PlayerId);

        UpdateMeetingButton();

        var action = _localMode == ArbitratorJudgmentMode.Accuse ? "<color=#FF6868>ACCUSE</color>" : "<color=#66BFFF>DEFEND</color>";

        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FFD166>Judgment locked:</color> {action} {target.Data.PlayerName}"));
    }

    [RegisterEvent]
    public static void OnMeetingEnd(EndMeetingEvent evt)
    {
        _localLocked = false;

        if (AmongUsClient.Instance.AmHost)
        {
            _hostOwner = byte.MaxValue;
            _hostTarget = byte.MaxValue;
        }
    }

    public static IEnumerator CoSetupMeetingButton(MeetingHud hud)
    {
        while (hud && hud.CurrentState == MeetingHud.MeetingStates.Animating)
            yield return null;

        yield return new WaitForSeconds(0.25f);

        if (!hud || PlayerControl.LocalPlayer.Data.Role is not ArbitratorRole)
            yield break;

        Coroutines.Start(CoroutinesHelper.CoNotify("Choose ACCUSE or DEFEND, then select a player now.\nAfter locking your judgment, vote normally."));
        UpdateMeetingButton();
        while (hud && hud.CurrentState is not (MeetingHud.MeetingStates.Results or MeetingHud.MeetingStates.Proceeding))
        {
            UpdateMeetingButton();
            yield return null;
        }
    }

    public static bool IsJudgmentOpen()
    {
        return MeetingHud.Instance && MeetingHud.Instance.discussionTimer < Mathf.Max(GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.DiscussionTime), OptionGroupSingleton<ArbitratorOptions>.Instance.JudgmentWindow) && MeetingHud.Instance.CurrentState is MeetingHud.MeetingStates.Discussion or MeetingHud.MeetingStates.NotVoted or MeetingHud.MeetingStates.Voted;
    }

    public static void UpdateMeetingButton()
    {
        if (!MeetingHud.Instance || PlayerControl.LocalPlayer.Data.Role is not ArbitratorRole)
            return;

        var button = MeetingHud.Instance.MeetingAbilityButton;

        button.Show();
        button.SetInfiniteUses();
        button.SetCoolDown(0f, 1f);
        button.graphic.sprite = _localMode == ArbitratorJudgmentMode.Accuse ? NewModAsset.AccuseButton.LoadAsset() : NewModAsset.DefendButton.LoadAsset();
        button.graphic.SetCooldownNormalizedUvs();

        if (_localLocked || !IsJudgmentOpen())
        {
            button.OverrideText(_localLocked ? "LOCKED" : "CLOSED");
            button.OverrideColor(new Color32(90, 90, 90, 255));
            return;
        }

        if (_localMode == ArbitratorJudgmentMode.Accuse)
        {
            button.OverrideText("ACCUSE\nSELECT");
            button.OverrideColor(new Color32(255, 104, 104, 255));
        }
        else
        {
            button.OverrideText("DEFEND\nSELECT");
            button.OverrideColor(new Color32(102, 191, 255, 255));
        }
    }

    [RegisterEvent]
    public static void OnPopulateResults(PopulateResultsEvent evt)
    {
        LastVotes.Clear();

        foreach (var vote in evt.Votes)
            LastVotes[vote.Voter] = vote.Suspect;

        if (!AmongUsClient.Instance.AmHost || _judgmentResolved || _hostOwner == byte.MaxValue || _hostTarget == byte.MaxValue) return;

        _judgmentResolved = true;

        var arbitrator = Utils.PlayerById(_hostOwner);

        if (!arbitrator || arbitrator.Data.IsDead || arbitrator.Data.Disconnected)
            return;

        var target = Utils.PlayerById(_hostTarget);
        if (!target || target.Data.IsDead || target.Data.Disconnected) return;
        var votesOnTarget = 0;

        foreach (var vote in evt.Votes)
        {
            if (vote.Suspect == _hostTarget && vote.Voter != _hostTarget)
                votesOnTarget++;
        }

        var exiled = MeetingHud.Instance.exiledPlayer;

        var success = _hostMode switch
        {
            ArbitratorJudgmentMode.Accuse => exiled != null && exiled.PlayerId == _hostTarget,

            ArbitratorJudgmentMode.Defend => (exiled == null || exiled.PlayerId != _hostTarget) && votesOnTarget >= OptionGroupSingleton<ArbitratorOptions>.Instance.DefendVotesRequired,

            _ => false,
        };

        JudgmentTokens.TryGetValue(arbitrator.PlayerId, out var tokens);

        if (success)
        {
            tokens++;
            ScoredTargets.Add(_hostTarget);
        }

        RpcResolveJudgment(PlayerControl.LocalPlayer, arbitrator.PlayerId, _hostTarget, success, tokens);

        if (success && tokens >= OptionGroupSingleton<ArbitratorOptions>.Instance.JudgmentTokensToWin) PendingWinner = arbitrator.PlayerId;
    }

    public static void OnMeetingAbilityClicked()
    {
        if (_localLocked || !IsJudgmentOpen())
        {
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#B7B7B7>Your judgment is already locked.</color>"));

            return;
        }

        _localMode = _localMode == ArbitratorJudgmentMode.Accuse ? ArbitratorJudgmentMode.Defend : ArbitratorJudgmentMode.Accuse;

        UpdateMeetingButton();
    }

    [MethodRpc((uint)CustomRPC.ArbitratorJudgment)]
    public static void RpcSetJudgment(PlayerControl source, byte mode, byte targetId)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not ArbitratorRole || source.Data.IsDead || source.Data.Disconnected || !IsJudgmentOpen() || _hostOwner != byte.MaxValue || mode > (byte)ArbitratorJudgmentMode.Defend || ScoredTargets.Contains(targetId))
            return;

        var target = Utils.PlayerById(targetId);
        if (!target || target == source || target.Data.IsDead || target.Data.Disconnected) return;
        _hostOwner = source.PlayerId;
        _hostTarget = targetId;
        _hostMode = (ArbitratorJudgmentMode)mode;
    }

    [MethodRpc((uint)CustomRPC.ArbitratorJudgmentResult)]
    public static void RpcResolveJudgment(PlayerControl source, byte ownerId, byte targetId, bool success, byte tokens)
    {
        if (!source.IsHost()) return;
        JudgmentTokens[ownerId] = tokens;
        if (success && tokens == OptionGroupSingleton<ArbitratorOptions>.Instance.JudgmentTokensToWin - 1)
            Coroutines.Start(CoroutinesHelper.CoNotify("An Arbitrator is one correct judgment away from winning."));
        if (success) ScoredTargets.Add(targetId);

        if (PlayerControl.LocalPlayer.PlayerId != ownerId)
            return;

        var required = (int)OptionGroupSingleton<ArbitratorOptions>.Instance.JudgmentTokensToWin;

        if (success)
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FFD166>Judgment upheld.</color>\nJudgment Tokens: {tokens}/{required}"));
        else
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#B7B7B7>Judgment failed.</color>\nJudgment Tokens: {tokens}/{required}"));
    }

    public static void ResetState()
    {
        PendingWinner = byte.MaxValue;
        JudgmentTokens.Clear();
        ScoredTargets.Clear();
        LastVotes.Clear();

        _localMode = ArbitratorJudgmentMode.Accuse;
        _localLocked = false;

        _hostOwner = byte.MaxValue;
        _hostTarget = byte.MaxValue;
        _hostMode = ArbitratorJudgmentMode.Accuse;
        _judgmentResolved = false;
    }
}
