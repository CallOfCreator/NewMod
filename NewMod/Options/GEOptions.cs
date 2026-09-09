using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
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

    public ModdedToggleOption EnableGeneralEvents { get; } = new("Enable General Events", true);

    public ModdedNumberOption MinimumInterval { get; } = new("Minimum Time Between Events", 50f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption MaximumInterval { get; } = new("Maximum Time Between Events", 80f, 15f, 180f, 5f, MiraNumberSuffixes.Seconds) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption EventTriggerChance { get; } = new("Chance to Start an Event", 65f, 0f, 100f, 5f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption AbilityExchangeFrequency { get; } = new("Ability Exchange Frequency", 15f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption CrimsonVortexFrequency { get; } = new("Crimson Vortex Frequency", 15f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption IdentityCrisisFrequency { get; } = new("Identity Crisis Frequency", 25f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption NegativeRealityFrequency { get; } = new("Negative Reality Frequency", 20f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption NoMansLandFrequency { get; } = new("No Man's Land Frequency", 10f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption RoleScrambleFrequency { get; } = new("Role Scramble Frequency", 0f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption ScreenDesynchronizationFrequency { get; } = new("Screen Desynchronization Frequency", 15f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption SystemOverrideFrequency { get; } = new("System Override Frequency", 15f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption CrimsonDuration { get; } = new("Vortex Duration", 30f, 10f, 60f, 5f, MiraNumberSuffixes.Seconds) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption CrimsonRadius { get; } = new("Vortex Size", 5f, 2f, 20f, 0.5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedEnumOption<VortexIntensity> CrimsonIntensity { get; } = new("Pull Strength", VortexIntensity.Standard) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedEnumOption<EscapeDifficulty> CrimsonEscapeDifficulty { get; } = new("Escape Difficulty", EscapeDifficulty.Standard) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };
}