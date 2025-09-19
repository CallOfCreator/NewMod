using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers
{
    public class DrowsyModifierOptions : AbstractOptionGroup<DrowsyModifier>
    {
        public override string GroupName => "Drowsy Settings";

        [ModdedNumberOption("Speed Multiplier", min: 0.2f, max: 0.9f, increment: 0.05f, MiraNumberSuffixes.Multiplier)]
        public float SpeedMultiplier { get; set; } = 0.5f;
    }
}
