using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers;

public class ExplosiveModifierOptions : AbstractOptionGroup<ExplosiveModifier>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.ExplosiveModifier");

    [ModdedNumberOption("NewMod.Options.ExplosiveModifier.KillDistance", 5f, 20f)]
    public float KillDistance { get; set; } = 10f;

    [ModdedNumberOption("NewMod.Options.ExplosiveModifier.Duration", 40f, 60f)]
    public float Duration { get; set; } = 50f;
}
