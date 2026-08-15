using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Modifiers.S1;

namespace NewMod.Options.Modifiers;

[MiraIgnore]
public class MarkedModifierOptions : AbstractOptionGroup<MarkedModifier>
{
    public override string GroupName => "Marked Settings";

    [ModdedNumberOption("Mark Range", min: 1f, max: 5f, increment: 0.25f, suffixType: MiraNumberSuffixes.None)]
    public float MarkRange { get; set; } = 2.5f;

    [ModdedNumberOption("Required Proximity Time", min: 1f, max: 10f, increment: 0.5f, suffixType: MiraNumberSuffixes.Seconds)]
    public float ProximityTime { get; set; } = 4f;

    [ModdedNumberOption("Indicator Duration", min: 1f, max: 8f, increment: 0.5f, suffixType: MiraNumberSuffixes.Seconds)]
    public float IndicatorDuration { get; set; } = 3f;
}