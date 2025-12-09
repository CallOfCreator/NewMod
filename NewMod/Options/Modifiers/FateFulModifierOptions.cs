using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers
{
    public class FatefulModifierOptions : AbstractOptionGroup<FatefulModifier>
    {
        public override string GroupName => "Fateful Modifier Settings";

        [ModdedNumberOption("Death Chance (%)", min: 1f, max: 100f, increment: 1f, suffixType:MiraNumberSuffixes.Percent)]
        public float DeathChance { get; set; } = 1f;
    }
}
