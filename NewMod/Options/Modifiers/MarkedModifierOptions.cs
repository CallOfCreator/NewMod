using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Modifiers.S1;

namespace NewMod.Options.Modifiers;

[MiraIgnore]
public class MarkedModifierOptions : AbstractOptionGroup<MarkedModifier>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.MarkedModifier");

    [ModdedNumberOption("NewMod.Options.MarkedModifier.MarkRange", 1f, 5f, 0.25f, MiraNumberSuffixes.None)]
    public float MarkRange { get; set; } = 2.5f;

    [ModdedNumberOption("NewMod.Options.MarkedModifier.ProximityTime", 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float ProximityTime { get; set; } = 4f;

    [ModdedNumberOption("NewMod.Options.MarkedModifier.IndicatorDuration", 1f, 8f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float IndicatorDuration { get; set; } = 3f;
}
