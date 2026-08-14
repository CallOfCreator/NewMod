using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers;

public class AdrenalineModifierOptions : AbstractOptionGroup<AdrenalineModifier>
{
    public override string GroupName => "Adrenaline Settings";

    [ModdedNumberOption("Speed Multiplier", 1.1f, 3f, 0.1f, MiraNumberSuffixes.Multiplier)]
    public float SpeedMultiplier { get; set; } = 1.5f;
}