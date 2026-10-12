using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using NewMod.Options;
using NewMod.Options.Modifiers;
using UnityEngine;

namespace NewMod.Modifiers;

public sealed class DrowsyModifier : GameModifier
{
    public float AppliedMultiplier = 1f;

    public override string ModifierName => "Drowsy";
    public override bool ShowInFreeplay => true;
    public override Color FreeplayFileColor => new(0.6f, 0.7f, 1f);

    public override string GetDescription()
    {
        return $"Move slower (x{OptionGroupSingleton<DrowsyModifierOptions>.Instance.SpeedMultiplier:0.##}).";
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ModifiersOptions>.Instance.DrowsyChance.Value;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ModifiersOptions>.Instance.DrowsyAmount;
    }

    public override void OnActivate()
    {
        AppliedMultiplier = OptionGroupSingleton<DrowsyModifierOptions>.Instance.SpeedMultiplier;
        Player.MyPhysics.Speed *= AppliedMultiplier;
    }

    public override void OnDeactivate()
    {
        Player.MyPhysics.Speed /= AppliedMultiplier;
        AppliedMultiplier = 1f;
    }

    public override void OnDeath(DeathReason reason)
    {
        OnDeactivate();
    }
}
