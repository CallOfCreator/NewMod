using System.Collections;
using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using NewMod.Options;
using NewMod.Options.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace NewMod.Modifiers.S1;

[MiraIgnore]
public class MarkedModifier : GameModifier, INewModModifier
{
    private readonly Dictionary<byte, float> _nearTimers = [];
    private readonly HashSet<byte> _nearby = [];
    private readonly List<byte> _toRemove = [];
    private readonly HashSet<byte> _triggered = [];

    public override string ModifierName => "Marked";
    public override bool HideOnUi => false;
    public override bool ShowInFreeplay => true;
    public ModifierFaction Faction => ModifierFaction.Murder;

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ModifiersOptions>.Instance.MarkedAmount;
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<ModifiersOptions>.Instance.MarkedChance;
    }

    public override string GetDescription()
    {
        var options = OptionGroupSingleton<MarkedModifierOptions>.Instance;
        return $"Stay near the same player for {options.ProximityTime} seconds and they briefly see an indicator pointing toward you.";
    }

    public override void OnActivate()
    {
        _nearTimers.Clear();
        _triggered.Clear();
        _nearby.Clear();
    }

    public override void OnDeactivate()
    {
        _nearTimers.Clear();
        _triggered.Clear();
        _nearby.Clear();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!Player.AmOwner)
            return;

        if (Player.Data.IsDead || Player.Data.Disconnected || Player.inVent || MeetingHud.Instance)
        {
            _nearTimers.Clear();
            _triggered.Clear();
            _nearby.Clear();
            return;
        }

        var options = OptionGroupSingleton<MarkedModifierOptions>.Instance;
        var ownerPosition = Player.GetTruePosition();

        _nearby.Clear();

        foreach (var target in PlayerControl.AllPlayerControls)
        {
            if (target == Player || target.Data.IsDead || target.Data.Disconnected || target.inVent)
                continue;

            if (Vector2.Distance(ownerPosition, target.GetTruePosition()) > options.MarkRange)
                continue;

            _nearby.Add(target.PlayerId);

            if (_triggered.Contains(target.PlayerId))
                continue;

            _nearTimers.TryGetValue(target.PlayerId, out var timer);
            timer += Time.fixedDeltaTime;

            if (timer < options.ProximityTime)
            {
                _nearTimers[target.PlayerId] = timer;
                continue;
            }

            _nearTimers.Remove(target.PlayerId);
            _triggered.Add(target.PlayerId);

            RpcShowMarkedIndicator(Player, target.PlayerId, options.IndicatorDuration);
        }

        _toRemove.Clear();

        foreach (var pair in _nearTimers)
            if (!_nearby.Contains(pair.Key))
                _toRemove.Add(pair.Key);

        foreach (var id in _toRemove)
            _nearTimers.Remove(id);

        _toRemove.Clear();

        foreach (var id in _triggered)
            if (!_nearby.Contains(id))
                _toRemove.Add(id);

        foreach (var id in _toRemove)
            _triggered.Remove(id);
    }

    [MethodRpc((uint)CustomRPC.MarkedReveal)]
    public static void RpcShowMarkedIndicator(PlayerControl source, byte targetId, float duration)
    {
        if (PlayerControl.LocalPlayer.PlayerId != targetId)
            return;

        Coroutines.Start(CoShowMarkedIndicator(source, duration));
    }

    private static IEnumerator CoShowMarkedIndicator(PlayerControl source, float duration)
    {
        var go = new GameObject($"MarkedIndicator_{source.PlayerId}") { layer = 5 };

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = NewModAsset.Arrow.LoadAsset();

        var arrow = go.AddComponent<ArrowBehaviour>();
        arrow.image = renderer;
        arrow.MaxScale = 0.8f;
        arrow.alwaysMaxSize = true;
        arrow.target = source.transform.position;
        arrow.enabled = false;

        var timer = 0f;

        while (source && !source.Data.IsDead && !source.Data.Disconnected && !MeetingHud.Instance && timer < duration)
        {
            arrow.target = source.transform.position;
            arrow.UpdatePosition();
            renderer.enabled = true;

            timer += Time.deltaTime;
            yield return null;
        }

        Object.Destroy(go);
    }
}