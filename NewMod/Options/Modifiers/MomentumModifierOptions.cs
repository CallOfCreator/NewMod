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

    [ModdedNumberOption("Time To Max Speed", 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float TimeToMaxSpeed { get; set; } = 5f;

    [ModdedNumberOption("Maximum Speed Bonus", 5f, 75f, 5f, MiraNumberSuffixes.Percent)]
    public float MaxSpeedBonus { get; set; } = 35f;
}