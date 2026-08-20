using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;

namespace NewMod.Options;

public class GEOptions : AbstractOptionGroup
{
    [MiraIgnore]
    public override string GroupName => "General Events";

    public override MenuCategory ParentMenu => MenuCategory.Game;

    [ModdedNumberOption("Minimum Event Interval", min: 10f, max: 120f, increment: 5f, suffixType: MiraNumberSuffixes.Seconds)]
    public float MinimumInterval { get; set; } = 20f;

    [ModdedNumberOption("Maximum Event Interval", min: 15f, max: 180f, increment: 5f, suffixType: MiraNumberSuffixes.Seconds)]
    public float MaximumInterval { get; set; } = 30f;

    [ModdedNumberOption("Crimson Vortex Duration", min: 10f, max: 60f, increment: 5f, suffixType: MiraNumberSuffixes.Seconds)]
    public float CrimsonDuration { get; set; } = 30f;

    [ModdedNumberOption("Crimson Vortex Radius", min: 2f, max: 20f, increment: 0.5f)]
    public float CrimsonRadius { get; set; } = 5f;

    [ModdedNumberOption("Crimson Vortex Kill Radius", min: 0.2f, max: 2f, increment: 0.1f)]
    public float CrimsonKillRadius { get; set; } = 0.6f;

    [ModdedNumberOption("Crimson Vortex Edge Pull", min: 0.1f, max: 2f, increment: 0.05f)]
    public float CrimsonEdgePull { get; set; } = 0.55f;

    [ModdedNumberOption("Crimson Vortex Pull Strength", min: 1f, max: 8f, increment: 0.25f)]
    public float CrimsonPullStrength { get; set; } = 4.25f;

    [ModdedNumberOption("Crimson Vortex Orbit Strength", min: 0f, max: 6f, increment: 0.25f)]
    public float CrimsonOrbitStrength { get; set; } = 2.25f;

    [ModdedNumberOption("Crimson Vortex Max Speed", min: 1f, max: 4f, increment: 0.1f)]
    public float CrimsonMaxSpeed { get; set; } = 2.1f;

    [ModdedNumberOption("Crimson Escape Zone", min: 50f, max: 95f, increment: 5f, suffixType: MiraNumberSuffixes.Percent)]
    public float CrimsonEscapeZone { get; set; } = 75f;

    [ModdedNumberOption("Crimson Escape Progress Per Press", min: 2f, max: 25f, increment: 1f, suffixType: MiraNumberSuffixes.Percent)]
    public float CrimsonEscapeProgressPerPress { get; set; } = 9f;

    [ModdedNumberOption("Crimson Escape Decay Delay", min: 0.05f, max: 0.5f, increment: 0.05f, suffixType: MiraNumberSuffixes.Seconds)]
    public float CrimsonEscapeDecayDelay { get; set; } = 0.15f;

    [ModdedNumberOption("Crimson Escape Decay Rate", min: 5f, max: 100f, increment: 5f, suffixType: MiraNumberSuffixes.Percent)]
    public float CrimsonEscapeDecayRate { get; set; } = 55f;

    [ModdedNumberOption("Crimson Escape Safe Distance", min: 0.5f, max: 5f, increment: 0.5f)]
    public float CrimsonEscapeSafeDistance { get; set; } = 1.5f;
}