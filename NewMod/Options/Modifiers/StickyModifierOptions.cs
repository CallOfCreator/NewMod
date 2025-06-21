using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers
{
    public class StickyModifierOptions : AbstractOptionGroup<StickyModifier>
    {
        public override string GroupName => "Sticky Settings";
        public ModdedToggleOption EnableModifier { get; } = new("Enable StickyModifier", true);
        public ModdedNumberOption StickyDuration { get; } =
            new(
                "Duration of Sticky Effect",
                15f,
                min: 10f,
                max: 30f,
                increment: 0.5f,
                suffixType: MiraNumberSuffixes.Seconds
            );
        public ModdedNumberOption StickyDistance { get; } =
            new(
                "Distance to trigger stickiness",
                1f,
                min: 1f,
                max: 3f,
                increment: 0.5f,
                suffixType: MiraNumberSuffixes.None
            );
    }
}
