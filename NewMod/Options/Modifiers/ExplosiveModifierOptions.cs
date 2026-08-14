using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers;

public class ExplosiveModifierOptions : AbstractOptionGroup<ExplosiveModifier>
{
    public override string GroupName => "Explosive Settings";

    [ModdedNumberOption("Kill Distance", 5f, 20f)]
    public float KillDistance { get; set; } = 10f;

    [ModdedNumberOption("Explosive duration", 40f, 60f)]
    public float Duration { get; set; } = 50f;
}