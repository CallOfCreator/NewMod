using System.Collections;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Translation;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using NewMod.Buttons.Roles;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using NewMod.Components;
using NewMod.Options.Roles;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Roles.ImpostorRoles;

public class Tyrant : ImpostorRole, INewModRole
{
    public enum ThroneOutcome
    {
        None,
        ChampionSideWin
    }

    public static byte ChampionId = byte.MaxValue;
    public static bool OfferAnswered;
    public static bool ApexThroneReady;
    public static bool ApexThroneOutcomeSet;
    public static ThroneOutcome Outcome;
    public static bool ChampionMeeting;
    public int Kills;
    public float NextPulse;

    public string RoleName => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.Tyrant");
    public string RoleDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.Tyrant.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.Tyrant.TabDescription");
    public Color RoleColor => new(0.78f, 0.10f, 0.16f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public NewModFaction Faction => NewModFaction.Apex;

    public CustomRoleConfiguration Configuration =>
        new(this)
        {
            MaxRoleCount = 1,
            Icon = NewModAsset.CrownIcon,
            CanGetKilled = true,
            UseVanillaKillButton = true,
            CanUseVent = true,
            TasksCountForProgress = false,
            CanUseSabotage = false,
            DefaultChance = 25,
            DefaultRoleCount = 1,
            CanModifyChance = true,
            RoleHintType = RoleHintType.RoleTab
        };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = INewModRole.GetRoleTabText(this);
        text.AppendLine();
        text.AppendLine(string.Format(MiraLocaleManager.Get("NewMod.Roles.ImpostorRoles.Tyrant.Tab.Progress"), Kills));
        return text;
    }

    public override bool DidWin(GameOverReason reason)
    {
        return reason == CustomGameOver.GameOverReason<TyrantGameOver>();
    }

    public static void ResetState()
    {
        CustomRoleSingleton<Tyrant>.Instance.Kills = 0;
        CustomRoleSingleton<Tyrant>.Instance.NextPulse = 0f;
        ChampionId = byte.MaxValue;
        OfferAnswered = false;
        ApexThroneReady = false;
        ApexThroneOutcomeSet = false;
        ChampionMeeting = false;
        Outcome = ThroneOutcome.None;
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (evt.Source.Data.Role is not Tyrant tyrant || !evt.Target.Data.IsDead) return;
        tyrant.Kills++;
        if (tyrant.Kills >= 4) ApexThroneReady = true;
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        ChampionMeeting = ApexThroneReady && OfferAnswered && Outcome == ThroneOutcome.ChampionSideWin;
    }

    [RegisterEvent]
    public static void OnMeetingResolved(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro || !GameManager.Instance.ShouldCheckForGameEnd || !AmongUsClient.Instance.AmHost || !ChampionMeeting) return;
        var champion = Utils.PlayerById(ChampionId);
        var tyrant = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(player => player.Data.Role is Tyrant);
        ApexThroneOutcomeSet = champion && tyrant && !champion.Data.IsDead && !champion.Data.Disconnected && !tyrant.Data.IsDead && !tyrant.Data.Disconnected;
    }

    [MethodRpc((uint)CustomRPC.NotifyChampion)]
    public static void RpcNotifyChampion(PlayerControl source, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Tyrant tyrant || tyrant.Kills < 4 || source.Data.IsDead || source.Data.Disconnected || !target || target == source || target.Data.IsDead || target.Data.Disconnected || ChampionId != byte.MaxValue || MeetingHud.Instance || ExileController.Instance) return;
        RpcConfirmOffer(PlayerControl.LocalPlayer, source.PlayerId, target.PlayerId);
    }

    [MethodRpc((uint)CustomRPC.TyrantConfirmOffer)]
    public static void RpcConfirmOffer(PlayerControl source, byte tyrantId, byte championId)
    {
        if (!source.IsHost()) return;
        ChampionId = championId;
        OfferAnswered = false;
        var tyrant = Utils.PlayerById(tyrantId);
        var champion = Utils.PlayerById(championId);
        Coroutines.Start(CoroutinesHelper.CoNotify($"{tyrant.Data.PlayerName} is the Tyrant and has offered {champion.Data.PlayerName} an alliance.\nThey can accept or reject."));
        if (champion.AmOwner)
        {
            CustomButtonSingleton<AcceptChampionButton>.Instance.SetActive(true, champion.Data.Role);
            CustomButtonSingleton<RejectChampionButton>.Instance.SetActive(true, champion.Data.Role);
            Coroutines.Start(CoroutinesHelper.CoNotify("Accept to win with the Tyrant if you both survive a meeting.\nReject to keep your own objective."));
        }
    }

    [MethodRpc((uint)CustomRPC.TyrantAnswerOffer)]
    public static void RpcAnswerOffer(PlayerControl source, bool accepted)
    {
        if (!AmongUsClient.Instance.AmHost || source.PlayerId != ChampionId || OfferAnswered || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ExileController.Instance) return;
        RpcConfirmAnswer(PlayerControl.LocalPlayer, accepted);
    }

    [MethodRpc((uint)CustomRPC.TyrantConfirmAnswer)]
    public static void RpcConfirmAnswer(PlayerControl source, bool accepted)
    {
        if (!source.IsHost()) return;
        OfferAnswered = true;
        if (PlayerControl.LocalPlayer.PlayerId == ChampionId)
        {
            CustomButtonSingleton<AcceptChampionButton>.Instance.SetActive(false, PlayerControl.LocalPlayer.Data.Role);
            CustomButtonSingleton<RejectChampionButton>.Instance.SetActive(false, PlayerControl.LocalPlayer.Data.Role);
        }

        Outcome = accepted ? ThroneOutcome.ChampionSideWin : ThroneOutcome.None;
        Coroutines.Start(CoroutinesHelper.CoNotify(accepted ? "The Champion accepted.\nExile or kill either ally before they survive a meeting." : "The Champion rejected the Tyrant's offer."));
    }

    [MethodRpc((uint)CustomRPC.FearPulse)]
    public static void RpcSpawnFearPulse(PlayerControl source, float x, float y)
    {
        if (!AmongUsClient.Instance.AmHost || source.Data.Role is not Tyrant tyrant || tyrant.Kills < 1 || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ExileController.Instance || Time.time < tyrant.NextPulse) return;
        tyrant.NextPulse = Time.time + OptionGroupSingleton<TyrantOptions>.Instance.PulseCooldown;
        RpcConfirmPulse(PlayerControl.LocalPlayer, source.PlayerId, source.GetTruePosition());
    }

    [MethodRpc((uint)CustomRPC.TyrantConfirmPulse)]
    public static void RpcConfirmPulse(PlayerControl source, byte ownerId, Vector2 position)
    {
        if (source.IsHost()) Coroutines.Start(CoPulse(Utils.PlayerById(ownerId), position));
    }

    public static IEnumerator CoPulse(PlayerControl owner, Vector2 position)
    {
        var options = OptionGroupSingleton<TyrantOptions>.Instance;
        var radius = options.FearPulseRadius + Mathf.Min(2, ((Tyrant)owner.Data.Role).Kills - 1) * 0.5f;
        var bubble = Utils.CreateSphere("TyrantPulse", new Vector3(position.x, position.y, -1f), radius, Color.red, options.PulseWarning + options.FearPulseDuration);
        yield return new WaitForSeconds(options.PulseWarning);
        if (!owner || owner.Data.IsDead || owner.Data.Disconnected || owner.Data.Role is not Tyrant || MeetingHud.Instance || ExileController.Instance)
        {
            Destroy(bubble);
            yield break;
        }

        var area = new GameObject("FearPulseArea").AddComponent<FearPulseArea>();
        area.transform.position = position;
        area.bubble = bubble.GetComponent<AreaBubble>();
        area.Init(owner.PlayerId, radius, options.FearPulseDuration, options.FearPulseSpeed);
    }
}