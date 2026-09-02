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

    [ModdedNumberOption("Mark Range", 1f, 5f, 0.25f, MiraNumberSuffixes.None)]
    public float MarkRange { get; set; } = 2.5f;

    [ModdedNumberOption("Required Proximity Time", 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float ProximityTime { get; set; } = 4f;

    [ModdedNumberOption("Indicator Duration", 1f, 8f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float IndicatorDuration { get; set; } = 3f;
}