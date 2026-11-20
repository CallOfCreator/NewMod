using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers;

public class FalseFormModifierOptions : AbstractOptionGroup<FalseFormModifier>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.FalseFormModifier");

    public ModdedNumberOption FalseFormDuration { get; } = new("NewMod.Options.FalseFormModifier.FalseFormDuration", 20f, 10f, 30f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption FalseFormAppearanceTimer { get; } = new("NewMod.Options.FalseFormModifier.FalseFormAppearanceTimer", 5f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption RevertAppearance { get; } = new("NewMod.Options.FalseFormModifier.RevertAppearance", true);
}
