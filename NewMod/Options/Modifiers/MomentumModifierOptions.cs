using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Modifiers.S1;

namespace NewMod.Options.Modifiers;

[MiraIgnore]
public class MomentumModifierOptions : AbstractOptionGroup<MomentumModifier>
{
    public override string GroupName => "Momentum Settings";

    [ModdedNumberOption("Time To Max Speed", min: 1f, max: 10f, increment: 0.5f, suffixType: MiraNumberSuffixes.Seconds)]
    public float TimeToMaxSpeed { get; set; } = 5f;

    [ModdedNumberOption("Maximum Speed Bonus", min: 5f, max: 75f, increment: 5f, suffixType: MiraNumberSuffixes.Percent)]
    public float MaxSpeedBonus { get; set; } = 35f;
}