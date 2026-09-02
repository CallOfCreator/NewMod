using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;

namespace NewMod.Options;

[MiraIgnore]
public class GEOptions : AbstractOptionGroup
{
    public enum EscapeDifficulty
    {
        Easy,
        Standard,
        Hard
    }

    public enum VortexIntensity
    {
        Gentle,
        Standard,
        Brutal
    }

    public override string GroupName => "General Events";

    public override MenuCategory ParentMenu => MenuCategory.Game;

    [ModdedToggleOption("Enable General Events")]
    public bool EnableGeneralEvents { get; set; } = true;

    [ModdedNumberOption("Minimum Event Interval", 10f, 120f, 5f, MiraNumberSuffixes.Seconds)]
    public float MinimumInterval { get; set; } = 50f;

    [ModdedNumberOption("Maximum Event Interval", 15f, 180f, 5f, MiraNumberSuffixes.Seconds)]
    public float MaximumInterval { get; set; } = 80f;

    [ModdedNumberOption("Event Trigger Chance", 0f, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float EventTriggerChance { get; set; } = 65f;

    [ModdedNumberOption("Ability Exchange Weight", 0f, 100f, 5f)]
    public float AbilityExchangeWeight { get; set; } = 15f;

    [ModdedNumberOption("Crimson Vortex Weight", 0f, 100f, 5f)]
    public float CrimsonVortexWeight { get; set; } = 15f;

    [ModdedNumberOption("Identity Crisis Weight", 0f, 100f, 5f)]
    public float IdentityCrisisWeight { get; set; } = 25f;

    [ModdedNumberOption("Negative Reality Weight", 0f, 100f, 5f)]
    public float NegativeRealityWeight { get; set; } = 20f;

    [ModdedNumberOption("No Man's Land Weight", 0f, 100f, 5f)]
    public float NoMansLandWeight { get; set; } = 10f;

    [ModdedNumberOption("Role Scramble Weight", 0f, 100f, 5f)]
    public float RoleScrambleWeight { get; set; } = 0f;

    [ModdedNumberOption("System Override Weight", 0f, 100f, 5f)]
    public float SystemOverrideWeight { get; set; } = 15f;

    [ModdedNumberOption("Crimson Vortex Duration", 10f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float CrimsonDuration { get; set; } = 30f;

    [ModdedNumberOption("Crimson Vortex Radius", 2f, 20f, 0.5f)]
    public float CrimsonRadius { get; set; } = 5f;

    [ModdedEnumOption("Crimson Vortex Intensity", typeof(VortexIntensity))]
    public VortexIntensity CrimsonIntensity { get; set; } = VortexIntensity.Standard;

    [ModdedEnumOption("Crimson Escape Difficulty", typeof(EscapeDifficulty))]
    public EscapeDifficulty CrimsonEscapeDifficulty { get; set; } = EscapeDifficulty.Standard;
}
