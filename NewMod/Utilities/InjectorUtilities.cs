using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using NewMod.Buttons.Roles;
using NewMod.Components;
using MiraAPI.Utilities;
using NewMod.Options.Roles;
using NewMod.RoleLogic;
using NewMod.Roles.NeutralRoles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;
using TMPro;

namespace NewMod.Utilities;

public static class InjectorUtilities
{
    public static readonly Dictionary<byte, InjectionSample> Experiments = [];
    public static readonly HashSet<(byte Owner, byte Target)> Samples = [];
    public static readonly Dictionary<byte, float> NextInjection = [];
    public static readonly HashSet<byte> Submitting = [];
    public static readonly HashSet<byte> Submitted = [];
    public static SerumType SelectedSerum;

    public static int SampleCount(byte ownerId)
    {
        return Samples.Count(sample => sample.Owner == ownerId);
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro) return;
        Reset();
    }

    [RegisterEvent]
    public static void OnGameEnd(GameEndEvent evt)
    {
        Reset();
    }

    public static void Reset()
    {
        Experiments.Clear();
        Samples.Clear();
        NextInjection.Clear();
        Submitting.Clear();
        Submitted.Clear();
        SelectedSerum = SerumType.Adrenaline;
    }

    public static void HostFixedUpdate()
    {
        foreach (var pair in Experiments.ToArray())
        {
            var owner = Utils.PlayerById(pair.Key);
            var target = Utils.PlayerById(pair.Value.TargetId);
            if (!owner || !target || owner.Data.IsDead || owner.Data.Disconnected || owner.Data.Role is not InjectorRole || target.Data.IsDead || target.Data.Disconnected || MeetingHud.Instance || ExileController.Instance || Time.time >= pair.Value.ExpiresAt)
                RpcResolveExperiment(PlayerControl.LocalPlayer, pair.Key, false);
        }
    }

    [MethodRpc((uint)CustomRPC.ApplySerum)]
    public static void RpcApplySerum(PlayerControl source, PlayerControl target, SerumType serum)
    {
        if ((!AmongUsClient.Instance.AmHost && !AmongUsClient.Instance.AmLocalHost) || source.Data.Role is not InjectorRole || source.Data.IsDead || source.Data.Disconnected || !target || target == source || target.Data.IsDead || target.Data.Disconnected || target.inVent || MeetingHud.Instance || ExileController.Instance || Experiments.ContainsKey(source.PlayerId) || Experiments.Values.Any(sample => sample.TargetId == target.PlayerId) || Samples.Contains((source.PlayerId, target.PlayerId)) || Time.time < NextInjection.GetValueOrDefault(source.PlayerId) || serum is not (SerumType.Adrenaline or SerumType.Sedative))
            return;
        var options = OptionGroupSingleton<InjectorOptions>.Instance;
        if (Vector2.Distance(source.GetTruePosition(), target.GetTruePosition()) > options.InjectionRange || PhysicsHelpers.AnythingBetween(source.GetTruePosition(), target.GetTruePosition(), Constants.ShipAndObjectsMask, false))
            return;
        RpcConfirmInjection(PlayerControl.LocalPlayer, source.PlayerId, target.PlayerId, serum);
    }

    [MethodRpc((uint)CustomRPC.InjectorConfirmInjection)]
    public static void RpcConfirmInjection(PlayerControl source, byte ownerId, byte targetId, SerumType serum)
    {
        if (!source.IsHost()) return;
        var options = OptionGroupSingleton<InjectorOptions>.Instance;
        Experiments[ownerId] = new InjectionSample { TargetId = targetId, Serum = serum, ReadyAt = Time.time + options.ObservationDuration, ExpiresAt = Time.time + options.ObservationDuration + options.CollectionWindow };
        NextInjection[ownerId] = Time.time + options.SerumCooldown;
        if (PlayerControl.LocalPlayer.PlayerId == targetId)
        {
            var button = CustomButtonSingleton<CleanseSerumButton>.Instance;
            button.EffectActive = false;
            button.Timer = 0f;
            button.SetActive(true, PlayerControl.LocalPlayer.Data.Role);
            Coroutines.Start(CoroutinesHelper.CoNotify(serum == SerumType.Adrenaline ? "Adrenaline injected: you move faster briefly.\nUse Cleanse to cancel the experiment." : "Sedative injected: you move slower briefly.\nUse Cleanse to cancel the experiment."));
        }
    }

    [MethodRpc((uint)CustomRPC.InjectorCollectSample)]
    public static void RpcCollectSample(PlayerControl source)
    {
        if ((!AmongUsClient.Instance.AmHost && !AmongUsClient.Instance.AmLocalHost) || source.Data.Role is not InjectorRole || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ExileController.Instance || !Experiments.TryGetValue(source.PlayerId, out var sample) || Time.time < sample.ReadyAt || Time.time >= sample.ExpiresAt)
            return;
        var target = Utils.PlayerById(sample.TargetId);
        if (!target || target.Data.IsDead || target.Data.Disconnected || target.inVent || Vector2.Distance(source.GetTruePosition(), target.GetTruePosition()) > OptionGroupSingleton<InjectorOptions>.Instance.InjectionRange || PhysicsHelpers.AnythingBetween(source.GetTruePosition(), target.GetTruePosition(), Constants.ShipAndObjectsMask, false))
            return;
        RpcResolveExperiment(PlayerControl.LocalPlayer, source.PlayerId, true);
    }

    [MethodRpc((uint)CustomRPC.InjectorCleanse)]
    public static void RpcCleanse(PlayerControl source)
    {
        if ((!AmongUsClient.Instance.AmHost && !AmongUsClient.Instance.AmLocalHost) || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ExileController.Instance) return;
        foreach (var pair in Experiments)
            if (pair.Value.TargetId == source.PlayerId)
            {
                RpcResolveExperiment(PlayerControl.LocalPlayer, pair.Key, false);
                break;
            }
    }

    [MethodRpc((uint)CustomRPC.InjectorResolveExperiment)]
    public static void RpcResolveExperiment(PlayerControl source, byte ownerId, bool collected)
    {
        if (!source.IsHost() || !Experiments.Remove(ownerId, out var sample)) return;
        if (PlayerControl.LocalPlayer.PlayerId == sample.TargetId)
        {
            var button = CustomButtonSingleton<CleanseSerumButton>.Instance;
            button.EffectActive = false;
            button.SetActive(false, PlayerControl.LocalPlayer.Data.Role);
        }

        if (collected) Samples.Add((ownerId, sample.TargetId));
        if (PlayerControl.LocalPlayer.PlayerId == ownerId)
            Coroutines.Start(CoroutinesHelper.CoNotify(collected ? $"<color=#75E6A5>Sample collected:</color> {SampleCount(ownerId)}/{OptionGroupSingleton<InjectorOptions>.Instance.RequiredInjectCount:0}." : "Experiment ended without a sample. You can try again."));
    }

    [MethodRpc((uint)CustomRPC.InjectorSubmit)]
    public static void RpcSubmit(PlayerControl source)
    {
        if ((!AmongUsClient.Instance.AmHost && !AmongUsClient.Instance.AmLocalHost) || source.Data.Role is not InjectorRole || source.Data.IsDead || source.Data.Disconnected || MeetingHud.Instance || ExileController.Instance || SampleCount(source.PlayerId) < OptionGroupSingleton<InjectorOptions>.Instance.RequiredInjectCount || Submitted.Contains(source.PlayerId) || source.inVent || !Submitting.Add(source.PlayerId))
            return;
        RpcAnnounceSubmission(PlayerControl.LocalPlayer, source.PlayerId);
        Coroutines.Start(CoSubmit(source));
    }

    [MethodRpc((uint)CustomRPC.InjectorAnnounceSubmission)]
    public static void RpcAnnounceSubmission(PlayerControl source, byte ownerId)
    {
        if (!source.IsHost()) return;
        var owner = Utils.PlayerById(ownerId);
        Submitting.Add(ownerId);
        Coroutines.Start(CoSubmissionWarning(owner));
    }

    public static IEnumerator CoSubmissionWarning(PlayerControl owner)
    {
        var duration = OptionGroupSingleton<InjectorOptions>.Instance.SubmissionDuration;
        var position = owner.GetTruePosition();
        var bubble = Utils.CreateSphere("InjectorSubmission", new Vector3(position.x, position.y, -1f), 1f, Color.green, duration, true);
        var hud = HudManager.Instance;
        var text = Helpers.CreateTextLabel("InjectorSubmissionWarning", hud.transform, AspectPosition.EdgeAlignments.Top, new Vector3(0f, 0.9f, -20f), 2.2f);
        text.color = new Color32(80, 255, 120, 255);
        text.fontStyle = FontStyles.Bold;
        var flash = Object.Instantiate(hud.FullScreen, hud.FullScreen.transform.parent);
        flash.name = "InjectorSubmissionFlash";
        flash.color = new Color(0.1f, 1f, 0.25f, 0.25f);
        flash.gameObject.SetActive(false);
        var end = Time.time + duration;
        var nextFlash = 0f;
        while (owner && Submitting.Contains(owner.PlayerId) && !MeetingHud.Instance && !ExileController.Instance && (AmongUsClient.Instance.IsGameStarted || DestroyableSingleton<TutorialManager>.InstanceExists))
        {
            var seconds = Mathf.Max(0, Mathf.CeilToInt(end - Time.time));
            text.text = owner.AmOwner ? $"STAY STILL AND SURVIVE - {seconds}s" : $"STOP THE INJECTOR - {seconds}s";
            if (Time.time >= nextFlash)
            {
                flash.gameObject.SetActive(!flash.gameObject.activeSelf);
                if (flash.gameObject.activeSelf && Constants.ShouldPlaySfx())
                    SoundManager.Instance.PlaySound(ShipStatus.Instance.SabotageSound, false, 0.7f);
                nextFlash = Time.time + 0.75f;
            }

            yield return null;
        }

        if (bubble)
            bubble.GetComponent<AreaBubble>().Break();
        Object.Destroy(text.gameObject);
        Object.Destroy(flash.gameObject);
    }

    [MethodRpc((uint)CustomRPC.InjectorFinishSubmission)]
    public static void RpcFinishSubmission(PlayerControl source, byte ownerId, bool completed)
    {
        if (!source.IsHost()) return;
        Submitting.Remove(ownerId);
        if (completed)
        {
            Submitted.Add(ownerId);
            if (DestroyableSingleton<TutorialManager>.InstanceExists && PlayerControl.LocalPlayer.PlayerId == ownerId)
                Coroutines.Start(CoroutinesHelper.CoNotify("<color=#75E6A5>Research complete.</color>\nYou met the Injector win condition."));
        }
        else if (PlayerControl.LocalPlayer.PlayerId == ownerId)
        {
            Coroutines.Start(CoroutinesHelper.CoNotify("<color=#FFB14F>Submission interrupted.</color>\nYour samples are kept."));
        }
    }

    public static IEnumerator CoSubmit(PlayerControl owner)
    {
        var ownerId = owner.PlayerId;
        var position = owner.GetTruePosition();
        var end = Time.time + OptionGroupSingleton<InjectorOptions>.Instance.SubmissionDuration;
        while (Submitting.Contains(ownerId) && Time.time < end)
        {
            if (!owner || owner.Data.IsDead || owner.Data.Disconnected || owner.Data.Role is not InjectorRole || MeetingHud.Instance || ExileController.Instance || Vector2.Distance(owner.GetTruePosition(), position) > 0.15f) break;
            yield return null;
        }

        if (!Submitting.Contains(ownerId)) yield break;
        var completed = owner && owner.Data.Role is InjectorRole && !owner.Data.IsDead && !owner.Data.Disconnected && !MeetingHud.Instance && !ExileController.Instance && Time.time >= end && Vector2.Distance(owner.GetTruePosition(), position) <= 0.15f;
        RpcFinishSubmission(PlayerControl.LocalPlayer, ownerId, completed);
    }
}