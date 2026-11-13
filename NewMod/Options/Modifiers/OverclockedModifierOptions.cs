using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Modifiers.S1;

namespace NewMod.Options.Modifiers;

[MiraIgnore]
public class OverclockedModifierOptions : AbstractOptionGroup<OverclockedModifier>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.OverclockedModifier");

    [ModdedNumberOption("NewMod.Options.OverclockedModifier.CooldownMultiplier", 0.5f, 1f, 0.05f, MiraNumberSuffixes.Multiplier)]
    public float CooldownMultiplier { get; set; } = 0.9f;

    [ModdedNumberOption("NewMod.Options.OverclockedModifier.PulseDuration", 0.25f, 2f, 0.25f, MiraNumberSuffixes.Seconds)]
    public float PulseDuration { get; set; } = 0.75f;
}
