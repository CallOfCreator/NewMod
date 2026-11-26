using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Modifiers.S1;

namespace NewMod.Options.Modifiers;

[MiraIgnore]
public class OverclockedModifierOptions : AbstractOptionGroup<OverclockedModifier>
{
    public override string GroupName => "Overclocked Settings";

    [ModdedNumberOption("Ability Cooldown Multiplier", 0.5f, 1f, 0.05f, MiraNumberSuffixes.Multiplier)]
    public float CooldownMultiplier { get; set; } = 0.9f;

    [ModdedNumberOption("Pulse Duration", 0.25f, 2f, 0.25f, MiraNumberSuffixes.Seconds)]
    public float PulseDuration { get; set; } = 0.75f;
}
