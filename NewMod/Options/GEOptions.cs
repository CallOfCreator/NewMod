using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;

namespace NewMod.Options;

public class GEOptions : AbstractOptionGroup
{
    public enum VortexIntensity
    {
        Gentle,
        Standard,
        Brutal
    }

    public enum EscapeDifficulty
    {
        Easy,
        Standard,
        Hard
    }

    [MiraIgnore]
    public override string GroupName => "General Events";

    public override MenuCategory ParentMenu => MenuCategory.Game;
    [ModdedToggleOption("Enable General Events")]
    public bool EnableGeneralEvents { get; set; } = true;

    [ModdedNumberOption("Minimum Event Interval", min: 10f, max: 120f, increment: 5f, suffixType: MiraNumberSuffixes.Seconds)]
    public float MinimumInterval { get; set; } = 20f;

    [ModdedNumberOption("Maximum Event Interval", min: 15f, max: 180f, increment: 5f, suffixType: MiraNumberSuffixes.Seconds)]
    public float MaximumInterval { get; set; } = 30f;

    [ModdedNumberOption("Crimson Vortex Duration", min: 10f, max: 60f, increment: 5f, suffixType: MiraNumberSuffixes.Seconds)]
    public float CrimsonDuration { get; set; } = 30f;

    [ModdedNumberOption("Crimson Vortex Radius", min: 2f, max: 20f, increment: 0.5f)]
    public float CrimsonRadius { get; set; } = 5f;

    [ModdedEnumOption("Crimson Vortex Intensity", typeof(VortexIntensity))]
    public VortexIntensity CrimsonIntensity { get; set; } = VortexIntensity.Standard;

    [ModdedEnumOption("Crimson Escape Difficulty", typeof(EscapeDifficulty))]
    public EscapeDifficulty CrimsonEscapeDifficulty { get; set; } = EscapeDifficulty.Standard;
}
