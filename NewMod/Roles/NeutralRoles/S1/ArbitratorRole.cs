using System.Collections;
using System.Collections.Generic;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using NewMod.Options.Roles.S1;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.NeutralRoles.S1;

public enum ArbitratorJudgmentMode : byte
{
    Accuse,
    Defend
}

[MiraIgnore]
public class ArbitratorRole : CrewmateRole, INewModRole
{
    public static readonly Dictionary<byte, byte> JudgmentTokens = [];
    public static readonly Dictionary<byte, byte> LastVotes = [];

    private static ArbitratorJudgmentMode _localMode;
    private static bool _localLocked;
    private static byte _localTarget = byte.MaxValue;

    private static byte _hostOwner = byte.MaxValue;
    private static byte _hostTarget = byte.MaxValue;
    private static ArbitratorJudgmentMode _hostMode;
    private static bool _judgmentResolved;

    public string RoleName => "Arbitrator";
    public string RoleDescription => "Accuse. Defend. Manipulate.";
    public string RoleLongDescription => "Secretly pass judgment during meetings.\n" + "Accuse a player and gain a Judgment Token if they are ejected, or Defend them and gain one if they survive despite receiving enough votes.\n" + "Use Leverage between meetings to learn whether someone voted with or against you.";

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
            GhostRole = AmongUs.GameOptions.RoleTypes.Crewmate,
            RoleHintType = RoleHintType.RoleTab
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var tabText = INewModRole.GetRoleTabText(this);
        JudgmentTokens.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var tokens);

        var required = (int)OptionGroupSingleton<ArbitratorOptions>.Instance.JudgmentTokensToWin;
        var defendVotes = (int)OptionGroupSingleton<ArbitratorOptions>.Instance.DefendVotesRequired;

        tabText.AppendLine();
        tabText.AppendLine($"<size=65%>Judgment Tokens: <color=#FFD166>{tokens}</color>/<color=#B7B7B7>{required}</color></size>");

        tabText.AppendLine("<size=65%><color=#FF6868>Accuse:</color> gain a token if your chosen player is ejected.</size>");

        tabText.AppendLine($"<size=65%><color=#66BFFF>Defend:</color> gain a token if they survive after receiving at least {defendVotes} votes.</size>");

        tabText.AppendLine("<size=65%><color=#FFD166>Leverage:</color> learn whether a nearby player voted with or against you last meeting.</size>");

        return tabText;
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
            ResetState();
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
        _localTarget = byte.MaxValue;
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
        if (_localLocked || PlayerControl.LocalPlayer.Data.Role is not ArbitratorRole)
            return;

        var target = Utils.PlayerById((byte)evt.TargetId);

        if (!target || target == PlayerControl.LocalPlayer || target.Data.IsDead || target.Data.Disconnected)
            return;

        evt.AllowSelect = false;

        _localLocked = true;
        _localTarget = target.PlayerId;

        RpcSetJudgment(PlayerControl.LocalPlayer, (byte)_localMode, target.PlayerId);

        UpdateMeetingButton();

        var action = _localMode == ArbitratorJudgmentMode.Accuse ? "<color=#FF6868>ACCUSE</color>" : "<color=#66BFFF>DEFEND</color>";

        Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FFD166>Judgment locked:</color> {action} {target.Data.PlayerName}"));
    }

    [RegisterEvent]
    public static void OnMeetingEnd(EndMeetingEvent evt)
    {
        _localLocked = false;
        _localTarget = byte.MaxValue;

        if (AmongUsClient.Instance.AmHost)
        {
            _hostOwner = byte.MaxValue;
            _hostTarget = byte.MaxValue;
        }
    }

    private static IEnumerator CoSetupMeetingButton(MeetingHud hud)
    {
        while (hud && hud.CurrentState == MeetingHud.MeetingStates.Animating)
            yield return null;

        yield return new WaitForSeconds(0.25f);

        if (!hud || PlayerControl.LocalPlayer.Data.Role is not ArbitratorRole)
            yield break;

        UpdateMeetingButton();

        Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FFD166>Arbitrator:</color> toggle ACCUSE/DEFEND\nthen select a player. Select again afterward to cast your normal vote"));
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

        if (_localLocked)
        {
            button.OverrideText("LOCKED");
            button.OverrideColor(new Color32(90, 90, 90, 255));
            return;
        }

        if (_localMode == ArbitratorJudgmentMode.Accuse)
        {
            button.OverrideText("ACCUSE");
            button.OverrideColor(new Color32(255, 104, 104, 255));
        }
        else
        {
            button.OverrideText("DEFEND");
            button.OverrideColor(new Color32(102, 191, 255, 255));
        }
    }

    [RegisterEvent]
    public static void OnPopulateResults(PopulateResultsEvent evt)
    {
        LastVotes.Clear();

        foreach (var vote in evt.Votes)
            LastVotes[vote.Voter] = vote.Suspect;

        if (!AmongUsClient.Instance.AmHost || _judgmentResolved || _hostOwner == byte.MaxValue || _hostTarget == byte.MaxValue)
        {
            return;
        }

        _judgmentResolved = true;

        var arbitrator = Utils.PlayerById(_hostOwner);

        if (!arbitrator || arbitrator.Data.Disconnected)
            return;

        var votesOnTarget = 0;

        foreach (var vote in evt.Votes)
        {
            if (vote.Suspect == _hostTarget)
                votesOnTarget++;
        }

        var exiled = MeetingHud.Instance.exiledPlayer;

        var success = _hostMode switch
        {
            ArbitratorJudgmentMode.Accuse => exiled != null && exiled.PlayerId == _hostTarget,

            ArbitratorJudgmentMode.Defend => (exiled == null || exiled.PlayerId != _hostTarget) && votesOnTarget >= OptionGroupSingleton<ArbitratorOptions>.Instance.DefendVotesRequired,

            _ => false
        };

        JudgmentTokens.TryGetValue(arbitrator.PlayerId, out var tokens);

        if (success)
            tokens++;

        RpcResolveJudgment(arbitrator, success, tokens);

        if (success && tokens >= OptionGroupSingleton<ArbitratorOptions>.Instance.JudgmentTokensToWin)
        {
            Coroutines.Start(CoTriggerWin(arbitrator.PlayerId));
        }
    }

    public static void OnMeetingAbilityClicked()
    {
        if (_localLocked)
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
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not ArbitratorRole)
            return;

        _hostOwner = source.PlayerId;
        _hostTarget = targetId;
        _hostMode = (ArbitratorJudgmentMode)mode;
    }

    [MethodRpc((uint)CustomRPC.ArbitratorJudgmentResult)]
    public static void RpcResolveJudgment(PlayerControl source, bool success, byte tokens)
    {
        JudgmentTokens[source.PlayerId] = tokens;

        if (!source.AmOwner)
            return;

        var required = (int)OptionGroupSingleton<ArbitratorOptions>.Instance.JudgmentTokensToWin;

        if (success)
        {
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#FFD166>Judgment upheld.</color>\nJudgment Tokens: {tokens}/{required}"));
        }
        else
        {
            Coroutines.Start(CoroutinesHelper.CoNotify($"<color=#B7B7B7>Judgment failed.</color>\nJudgment Tokens: {tokens}/{required}"));
        }
    }

    private static IEnumerator CoTriggerWin(byte playerId)
    {
        while (MeetingHud.Instance || ExileController.Instance)
            yield return null;

        if (!AmongUsClient.Instance.AmHost || !GameManager.Instance.ShouldCheckForGameEnd)
        {
            yield break;
        }

        var winner = Utils.PlayerById(playerId);

        if (winner && !winner.Data.Disconnected)
            CustomGameOver.Trigger<ArbitratorGameOver>([winner.Data]);
    }

    private static void ResetState()
    {
        JudgmentTokens.Clear();
        LastVotes.Clear();

        _localMode = ArbitratorJudgmentMode.Accuse;
        _localLocked = false;
        _localTarget = byte.MaxValue;

        _hostOwner = byte.MaxValue;
        _hostTarget = byte.MaxValue;
        _hostMode = ArbitratorJudgmentMode.Accuse;
        _judgmentResolved = false;
    }
}