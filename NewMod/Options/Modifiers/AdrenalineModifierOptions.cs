using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers
{
    public class AdrenalineModifierOptions : AbstractOptionGroup<AdrenalineModifier>
    {
        public override string GroupName => "Adrenaline Settings";

        [ModdedNumberOption("Speed Multiplier", min: 1.1f, max: 3f, increment: 0.1f, MiraNumberSuffixes.Multiplier)]
        public float SpeedMultiplier { get; set; } = 1.5f;
    }
}
