using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers;

public class StickyModifierOptions : AbstractOptionGroup<StickyModifier>
{
    public override string GroupName => "Sticky Settings";

    public ModdedNumberOption StickyDuration { get; } = new("Duration of Sticky Effect", 15f, 10f, 30f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption StickyDistance { get; } = new("Distance to trigger stickiness", 1f, 1f, 3f, 0.5f, MiraNumberSuffixes.None);

    public ModdedNumberOption PullStrength { get; } = new("Pull Strength", 1f, 1f, 5f, 1f, MiraNumberSuffixes.None);
}