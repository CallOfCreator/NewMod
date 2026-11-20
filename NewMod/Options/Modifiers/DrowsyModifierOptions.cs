using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers;

public class DrowsyModifierOptions : AbstractOptionGroup<DrowsyModifier>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.DrowsyModifier");

    [ModdedNumberOption("NewMod.Options.DrowsyModifier.SpeedMultiplier", 0.2f, 0.9f, 0.05f, MiraNumberSuffixes.Multiplier)]
    public float SpeedMultiplier { get; set; } = 0.5f;
}
