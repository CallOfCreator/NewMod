using System.Collections;
using System.Collections.Generic;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using NewMod.Options;
using NewMod.Options.Modifiers;
using NewMod.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Modifiers;

public class StickyModifier : GameModifier
{
    public static readonly Dictionary<byte, byte> ActiveLinks = [];
    public static readonly HashSet<byte> LinkedTargets = [];

    private float _nextRequestAt;

    public override string ModifierName => "Sticky";
    public override bool HideOnUi => false;
    public override bool ShowInFreeplay => true;

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ModifiersOptions>.Instance.StickyAmount;
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<ModifiersOptions>.Instance.StickyChance;
    }

    public override bool? CanVent()
    {
        return Player.Data.Role.CanVent;
    }

    public override string GetDescription()
    {
        var distance = OptionGroupSingleton<StickyModifierOptions>.Instance.StickyDistance.Value;
        var duration = OptionGroupSingleton<StickyModifierOptions>.Instance.StickyDuration.Value;

        return $"{ModifierName}: Sticks to a nearby player within {distance:0.#} units and pulls them along for {duration:0.#} seconds.";
    }

    public override void OnDeactivate()
    {
        if (!ActiveLinks.TryGetValue(Player.PlayerId, out var targetId))
            return;

        ActiveLinks.Remove(Player.PlayerId);
        LinkedTargets.Remove(targetId);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (ActiveLinks.TryGetValue(Player.PlayerId, out var linkedTargetId))
        {
            var target = Utils.PlayerById(linkedTargetId);

            if (!target || !target.AmOwner || Player.Data == null || target.Data == null || Player.Data.IsDead || Player.Data.Disconnected || target.Data.IsDead || target.Data.Disconnected || Player.inVent || target.inVent || MeetingHud.Instance || !target.CanMove) return;

            const float stopDistance = 0.65f;

            var sourcePosition = Player.GetTruePosition();
            var targetBodyPosition = target.MyPhysics.body.position;
            var targetPosition = targetBodyPosition + target.Collider.offset;
            var delta = sourcePosition - targetPosition;
            var distance = delta.magnitude;

            if (distance <= stopDistance)
                return;

            var direction = delta / distance;
            var stretch = distance - stopDistance;

            var options = OptionGroupSingleton<StickyModifierOptions>.Instance;
            var pullStrength = Mathf.Max(0.1f, options.PullStrength.Value);
            var triggerDistance = Mathf.Max(stopDistance + 0.1f, options.StickyDistance.Value);

            var tension = Mathf.Clamp01(stretch / (triggerDistance - stopDistance));
            var pullSpeed = pullStrength * Mathf.Lerp(0.35f, 1.5f, tension);

            if (distance > triggerDistance * 1.5f)
                pullSpeed *= 1.5f;

            var leashPoint = sourcePosition - direction * stopDistance;
            var nextPosition = Vector2.MoveTowards(targetPosition, leashPoint, pullSpeed * Time.fixedDeltaTime);

            target.MyPhysics.body.position = nextPosition - target.Collider.offset;
            return;
        }

        if (!Player.AmOwner)
            return;

        if (!Player.CanMove || Player.Data == null || Player.Data.IsDead || Player.Data.Disconnected || Player.inVent || MeetingHud.Instance) return;

        if (Time.time < _nextRequestAt)
            return;

        var range = OptionGroupSingleton<StickyModifierOptions>.Instance.StickyDistance.Value;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!IsValidTarget(Player, player))
                continue;

            if (Vector2.Distance(Player.GetTruePosition(), player.GetTruePosition()) > range)
                continue;

            _nextRequestAt = Time.time + 0.25f;
            RpcRequestSticky(Player);
            break;
        }
    }

    private static bool IsValidTarget(PlayerControl source, PlayerControl target)
    {
        return target && target != source && !target.Data.IsDead && !target.Data.Disconnected && !target.inVent && !LinkedTargets.Contains(target.PlayerId);
    }

    private static PlayerControl FindClosestTarget(PlayerControl source, float range)
    {
        PlayerControl closest = null;
        var closestDistance = range;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!IsValidTarget(source, player))
                continue;

            var distance = Vector2.Distance(source.GetTruePosition(), player.GetTruePosition());

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closest = player;
        }

        return closest;
    }

    [MethodRpc((uint)CustomRPC.StickyRequestLink)]
    public static void RpcRequestSticky(PlayerControl source)
    {
        if (!AmongUsClient.Instance.AmHost || !source || source.Data.Disconnected || source.inVent || ActiveLinks.ContainsKey(source.PlayerId)) return;

        var range = OptionGroupSingleton<StickyModifierOptions>.Instance.StickyDistance.Value;
        var target = FindClosestTarget(source, range + 0.15f);

        if (!target)
            return;

        RpcStartSticky(source, target.PlayerId);

        var duration = OptionGroupSingleton<StickyModifierOptions>.Instance.StickyDuration.Value;
        Coroutines.Start(CoHostStickyTimer(source, target.PlayerId, duration));
    }

    [MethodRpc((uint)CustomRPC.StickyStartLink)]
    public static void RpcStartSticky(PlayerControl source, byte targetId)
    {
        if (!source)
            return;

        var target = Utils.PlayerById(targetId);

        if (!target)
            return;

        ActiveLinks[source.PlayerId] = targetId;
        LinkedTargets.Add(targetId);
    }

    [MethodRpc((uint)CustomRPC.StickyEndLink)]
    public static void RpcEndSticky(PlayerControl source, byte targetId)
    {
        if (!source)
            return;

        if (ActiveLinks.TryGetValue(source.PlayerId, out var currentTarget) && currentTarget == targetId) ActiveLinks.Remove(source.PlayerId);

        LinkedTargets.Remove(targetId);

        if (source.AmOwner && source.HasModifier<StickyModifier>())
            source.RpcRemoveModifier<StickyModifier>();
    }

    private static IEnumerator CoHostStickyTimer(PlayerControl source, byte targetId, float duration)
    {
        var timer = 0f;

        while (timer < duration)
        {
            if (!AmongUsClient.Instance.AmHost)
                yield break;

            var target = Utils.PlayerById(targetId);

            if (source.Data.IsDead || source.Data.Disconnected || target.Data.IsDead || target.Data.Disconnected || source.inVent || target.inVent || MeetingHud.Instance) break;

            if (!ActiveLinks.TryGetValue(source.PlayerId, out var linkedId) || linkedId != targetId) yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        if (!AmongUsClient.Instance.AmHost)
            yield break;

        if (ActiveLinks.TryGetValue(source.PlayerId, out var linkedTarget) && linkedTarget == targetId) RpcEndSticky(source, targetId);
    }

    public static void ResetState()
    {
        ActiveLinks.Clear();
        LinkedTargets.Clear();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro || (Application.platform == RuntimePlatform.Android && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay))
            return;

        ResetState();
    }
}