using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using NewMod.Achievements;
using NewMod.Components;
using NewMod.Options.Roles.S1;
using NewMod.Roles.CrewmateRoles.S1;
using NewMod.Seasons;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NewMod.Utilities;

public enum VerifierClaimType : byte
{
    None,
    DidTask,
    EnteredVent,
    NearBody,
    UsedAbility
}

[Flags]
public enum VerifierFactFlags : byte
{
    None = 0,
    DidTask = 1,
    EnteredVent = 2,
    NearBody = 4,
    UsedAbility = 8
}

public static class VerifierUtilities
{
    public static readonly Dictionary<byte, VerifierFactFlags> RoundFacts = new();

    public static bool SelectingPlayer;
    public static bool UsedThisMeeting;

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro || (Application.platform == RuntimePlatform.Android && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay))
            return;

        Reset(true);
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        Reset(true);
    }

    [RegisterEvent]
    public static void OnCompleteTask(CompleteTaskEvent evt)
    {
        PublishFact(evt.Player, VerifierClaimType.DidTask);
    }

    [RegisterEvent]
    public static void OnEnterVent(EnterVentEvent evt)
    {
        if (!evt.IsCancelled)
            PublishFact(evt.Player, VerifierClaimType.EnteredVent);
    }

    [RegisterEvent]
    public static void OnMiraButtonClick(MiraButtonClickEvent evt)
    {
        var player = PlayerControl.LocalPlayer;

        if (evt.IsCancelled || !player || MeetingHud.Instance || ExileController.Instance || !evt.Button.CanClick())
            return;

        PublishFact(player, VerifierClaimType.UsedAbility);
    }

    [RegisterEvent]
    public static void OnVanillaButtonClick(VanillaButtonClickEvent evt)
    {
        var player = PlayerControl.LocalPlayer;

        if (evt.IsCancelled || !player || MeetingHud.Instance || ExileController.Instance)
            return;

        PublishFact(player, VerifierClaimType.UsedAbility);
    }

    [RegisterEvent]
    public static void OnReportBody(ReportBodyEvent evt)
    {
        if (AmongUsClient.Instance.AmHost)
            RegisterNearBody();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        Reset(hideMeetingButton: false);

        var player = PlayerControl.LocalPlayer;
        if (!player || player.Data.IsDead || player.Data.Disconnected || player.Data.Role is not VerifierRole)
            return;

        Coroutines.Start(CoSetupMeetingButton(evt.MeetingHud));
    }

    [RegisterEvent]
    public static void OnEndMeeting(EndMeetingEvent evt)
    {
        if (Minigame.Instance is VerifyMinigame minigame)
            minigame.ForceClose();

        Reset(true);
    }

    [RegisterEvent]
    public static void OnMeetingSelect(MeetingSelectEvent evt)
    {
        if (!SelectingPlayer || UsedThisMeeting)
            return;

        var player = PlayerControl.LocalPlayer;
        if (!player || player.Data.Role is not VerifierRole)
            return;

        var target = Utils.PlayerById((byte)evt.TargetId);
        if (!target || target.Data.IsDead || target.Data.Disconnected)
            return;

        evt.AllowSelect = false;
        SelectingPlayer = false;
        UpdateMeetingButton();

        VerifyMinigame.CreateMinigame(target);
    }

    [MethodRpc((uint)CustomRPC.VerifierRegisterFact)]
    public static void RpcRegisterFact(PlayerControl source, byte claimType)
    {
        if (!source || !source.Data || source.Data.Disconnected || claimType is < (byte)VerifierClaimType.DidTask or > (byte)VerifierClaimType.UsedAbility)
            return;

        RegisterFact(source.PlayerId, (VerifierClaimType)claimType);
    }

    public static void PublishFact(PlayerControl player, VerifierClaimType claim)
    {
        if (!player || !player.AmOwner || !player.Data || player.Data.IsDead || player.Data.Disconnected || !AmongUsClient.Instance || !AmongUsClient.Instance.IsGameStarted || MeetingHud.Instance || ExileController.Instance)
            return;

        var flag = GetFlag(claim);
        if (flag == VerifierFactFlags.None || (RoundFacts.TryGetValue(player.PlayerId, out var facts) && (facts & flag) != 0))
            return;

        RpcRegisterFact(player, (byte)claim);
    }

    [MethodRpc((uint)CustomRPC.VerifierRegisterNearBody)]
    public static void RpcRegisterNearBody(PlayerControl host, byte playerId)
    {
        if (!host || !host.IsHost())
            return;

        RegisterFact(playerId, VerifierClaimType.NearBody);
    }

    public static void RegisterFact(byte playerId, VerifierClaimType claimType)
    {
        var flag = GetFlag(claimType);

        if (flag == VerifierFactFlags.None)
            return;

        RoundFacts.TryGetValue(playerId, out var current);
        RoundFacts[playerId] = current | flag;
    }

    public static void RegisterNearBody()
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost || !PlayerControl.LocalPlayer)
            return;

        var radius = OptionGroupSingleton<VerifierOptions>.Instance.NearBodyRadius;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player || !player.Data || player.Data.IsDead || player.Data.Disconnected || (RoundFacts.TryGetValue(player.PlayerId, out var facts) && (facts & VerifierFactFlags.NearBody) != 0))
                continue;

            var bodies = Helpers.GetNearestDeadBodies(player.GetTruePosition(), radius, Helpers.CreateFilter(Constants.NotShipMask));

            if (bodies != null && bodies.Count > 0)
                RpcRegisterNearBody(PlayerControl.LocalPlayer, player.PlayerId);
        }
    }

    public static IEnumerator CoSetupMeetingButton(MeetingHud hud)
    {
        while (hud && hud.CurrentState == MeetingHud.MeetingStates.Animating)
            yield return null;

        yield return new WaitForSeconds(0.25f);

        if (!hud || PlayerControl.LocalPlayer.Data.Role is not VerifierRole)
            yield break;

        UpdateMeetingButton();
    }

    public static void UpdateMeetingButton()
    {
        if (!MeetingHud.Instance || !PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.Role is not VerifierRole)
            return;

        var button = MeetingHud.Instance.MeetingAbilityButton;

        button.Show();
        button.SetInfiniteUses();
        button.SetCoolDown(0f, 1f);
        button.graphic.sprite = NewModAsset.VerifyButton.LoadAsset();
        button.graphic.SetCooldownNormalizedUvs();

        button.OverrideText(UsedThisMeeting ? "USED" : SelectingPlayer ? "SELECT" : "VERIFY");
        button.OverrideColor(UsedThisMeeting ? new Color32(90, 90, 90, 255) : SelectingPlayer ? new Color32(255, 209, 102, 255) : new Color32(88, 232, 190, 255));
    }

    public static void OnMeetingAbilityClicked()
    {
        if (UsedThisMeeting)
        {
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#B7B7B7>Verifier already used this meeting.</color>"));
            return;
        }

        SelectingPlayer = !SelectingPlayer;
        UpdateMeetingButton();

        Coroutines.Start(CoroutinesHelper.CoNotify(SelectingPlayer ? "<color=#58E8BE>Verifier:</color> choose a player." : "<color=#B7B7B7>Verifier selection cancelled.</color>"));
    }

    public static string GetVerificationResult(PlayerControl target, VerifierClaimType claim, bool expected)
    {
        if (Random.Range(0f, 100f) < OptionGroupSingleton<VerifierOptions>.Instance.UnknownChance)
            return "<color=#B7B7B7>Unknown</color>";

        RoundFacts.TryGetValue(target.PlayerId, out var facts);
        var happened = (facts & GetFlag(claim)) != 0;

        if (Application.platform != RuntimePlatform.Android && SeasonManager.AvailableAchievementTabTypes.Contains(typeof(PreseasonAchievementsTab)))
            PreseasonAchievementsTab.TrustButVerify.Unlock();

        return happened == expected ? "<color=#58E8BE>Confirmed</color>" : "<color=#FF4D4D>Denied</color>";
    }

    public static VerifierFactFlags GetFlag(VerifierClaimType claimType)
    {
        return claimType switch
        {
            VerifierClaimType.DidTask => VerifierFactFlags.DidTask,
            VerifierClaimType.EnteredVent => VerifierFactFlags.EnteredVent,
            VerifierClaimType.NearBody => VerifierFactFlags.NearBody,
            VerifierClaimType.UsedAbility => VerifierFactFlags.UsedAbility,
            _ => VerifierFactFlags.None
        };
    }

    public static IEnumerator CoNotifyAfterDelay(float delay, string message)
    {
        yield return new WaitForSeconds(delay);
        Coroutines.Start(CoroutinesHelper.CoNotify(message));
    }

    public static void Reset(bool clearFacts = false, bool hideMeetingButton = true)
    {
        if (clearFacts)
            RoundFacts.Clear();

        SelectingPlayer = false;
        UsedThisMeeting = false;

        if (hideMeetingButton && MeetingHud.Instance && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data.Role is VerifierRole)
            MeetingHud.Instance.MeetingAbilityButton.Hide();
    }
}