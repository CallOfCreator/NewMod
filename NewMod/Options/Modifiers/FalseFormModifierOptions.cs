using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers;

public class FalseFormModifierOptions : AbstractOptionGroup<FalseFormModifier>
{
    public override string GroupName => "FalseForm Settings";

    public ModdedNumberOption FalseFormDuration { get; } = new("Duration of the FalseForm effect", 20f, 10f, 30f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption FalseFormAppearanceTimer { get; } = new("Appearance Change Delay", 5f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption RevertAppearance { get; } = new("Revert appearance after FalseForm ends", true);
}