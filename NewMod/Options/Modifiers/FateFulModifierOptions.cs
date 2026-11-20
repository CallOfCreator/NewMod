using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Modifiers.S1;

namespace NewMod.Options.Modifiers;

public class FatefulModifierOptions : AbstractOptionGroup<FatefulModifier>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.FatefulModifier");

    [ModdedNumberOption("NewMod.Options.FatefulModifier.DeathChance", 1f, 100f, 1f, MiraNumberSuffixes.Percent)]
    public float DeathChance { get; set; } = 1f;
}
