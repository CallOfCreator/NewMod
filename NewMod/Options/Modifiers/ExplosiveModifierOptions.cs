using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using NewMod.Modifiers;

namespace NewMod.Options.Modifiers
{
    public class ExplosiveModifierOptions : AbstractOptionGroup<ExplosiveModifier>
    {
        public override string GroupName => "Explosive Settings";

        [ModdedNumberOption("Kill Distance", min: 5f, max: 20f, increment: 1f, MiraNumberSuffixes.None)]
        public float KillDistance { get; set; } = 10f;

        [ModdedNumberOption("Explosive duration", min: 40f, max: 60f, increment: 1f, MiraNumberSuffixes.None)]
        public float Duration { get; set; } = 50f;
    }
}