using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers;

public class StickyModifierOptions : AbstractOptionGroup<StickyModifier>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.StickyModifier");

    public ModdedNumberOption StickyDuration { get; } = new("NewMod.Options.StickyModifier.StickyDuration", 15f, 10f, 30f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption StickyDistance { get; } = new("NewMod.Options.StickyModifier.StickyDistance", 1f, 1f, 3f, 0.5f, MiraNumberSuffixes.None);

    public ModdedNumberOption PullStrength { get; } = new("NewMod.Options.StickyModifier.PullStrength", 1f, 1f, 5f, 1f, MiraNumberSuffixes.None);
}
