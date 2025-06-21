using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers
{
    public class FalseFormModifierOptions : AbstractOptionGroup<FalseFormModifier>
    {
        public override string GroupName => "FalseForm Settings";
        public ModdedToggleOption EnableModifier { get; } = new("Enable FalseForm", true);
        public ModdedNumberOption FalseFormDuration { get; } =
            new(
                "Duration of the FalseForm effect",
                20f,
                min: 10f,
                max: 30f,
                increment: 1f,
                suffixType: MiraNumberSuffixes.Seconds
            );
        public ModdedNumberOption FalseFormAppearanceTimer { get; } =
            new(
                "Appearance Change Delay",
                5f,
                min: 1f,
                max: 10f,
                increment: 0.5f,
                suffixType: MiraNumberSuffixes.Seconds
            );
        public ModdedToggleOption RevertAppearance { get; } =
            new("Revert appearance after FalseForm ends", true);
    }
}
