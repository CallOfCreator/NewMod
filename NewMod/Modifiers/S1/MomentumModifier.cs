using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using NewMod.Options;
using NewMod.Options.Modifiers;
using UnityEngine;

namespace NewMod.Modifiers.S1;

[MiraIgnore]
public class MomentumModifier : GameModifier, INewModModifier
{
    private float _appliedMultiplier = 1f;
    private float _movingTime;

    public override string ModifierName => "Momentum";
    public override bool HideOnUi => false;
    public override bool ShowInFreeplay => true;
    public ModifierFaction Faction => ModifierFaction.Crew;

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ModifiersOptions>.Instance.MomentumAmount;
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<ModifiersOptions>.Instance.MomentumChance;
    }

    public override string GetDescription()
    {
        var options = OptionGroupSingleton<MomentumModifierOptions>.Instance;
        return $"Continuously moving builds up to {options.MaxSpeedBonus}% bonus speed over {options.TimeToMaxSpeed} seconds. Stopping resets it.";
    }

    public override void OnActivate()
    {
        _movingTime = 0f;
        _appliedMultiplier = 1f;
    }

    public override void OnDeactivate()
    {
        if (Player.AmOwner && _appliedMultiplier > 0f)
            Player.MyPhysics.Speed /= _appliedMultiplier;

        _movingTime = 0f;
        _appliedMultiplier = 1f;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!Player.AmOwner)
            return;

        var baseSpeed = Player.MyPhysics.Speed / Mathf.Max(_appliedMultiplier, 0.01f);

        if (Player.Data.IsDead || Player.Data.Disconnected || !Player.CanMove || Player.inVent || MeetingHud.Instance)
        {
            Player.MyPhysics.Speed = baseSpeed;
            _movingTime = 0f;
            _appliedMultiplier = 1f;
            return;
        }

        if (Player.MyPhysics.body.velocity.sqrMagnitude <= 0.01f)
        {
            Player.MyPhysics.Speed = baseSpeed;
            _movingTime = 0f;
            _appliedMultiplier = 1f;
            return;
        }

        var options = OptionGroupSingleton<MomentumModifierOptions>.Instance;

        _movingTime += Time.fixedDeltaTime;

        var progress = Mathf.Clamp01(_movingTime / options.TimeToMaxSpeed);
        var maxMultiplier = 1f + options.MaxSpeedBonus / 100f;

        _appliedMultiplier = Mathf.Lerp(1f, maxMultiplier, progress);
        Player.MyPhysics.Speed = baseSpeed * _appliedMultiplier;
    }
}