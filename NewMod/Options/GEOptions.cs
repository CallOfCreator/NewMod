using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.PluginLoading;
using MiraAPI.Translation;
using MiraAPI.Utilities;

namespace NewMod.Options;

[MiraIgnore]
public class GEOptions : AbstractOptionGroup
{
    public enum EscapeDifficulty
    {
        Easy,
        Standard,
        Hard,
    }

    public enum VortexIntensity
    {
        Gentle,
        Standard,
        Brutal,
    }

    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.GE");

    public override MenuCategory ParentMenu => MenuCategory.Game;

    public ModdedToggleOption EnableGeneralEvents { get; } = new("NewMod.Options.GE.EnableGeneralEvents", true);

    public ModdedNumberOption MinimumInterval { get; } = new("NewMod.Options.GE.MinimumInterval", 50f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption MaximumInterval { get; } = new("NewMod.Options.GE.MaximumInterval", 80f, 15f, 180f, 5f, MiraNumberSuffixes.Seconds) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption EventTriggerChance { get; } = new("NewMod.Options.GE.EventTriggerChance", 65f, 0f, 100f, 5f, MiraNumberSuffixes.Percent) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption AbilityExchangeFrequency { get; } = new("NewMod.Options.GE.AbilityExchangeFrequency", 15f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption CrimsonVortexFrequency { get; } = new("NewMod.Options.GE.CrimsonVortexFrequency", 15f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption IdentityCrisisFrequency { get; } = new("NewMod.Options.GE.IdentityCrisisFrequency", 25f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption NegativeRealityFrequency { get; } = new("NewMod.Options.GE.NegativeRealityFrequency", 20f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption NoMansLandFrequency { get; } = new("NewMod.Options.GE.NoMansLandFrequency", 10f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption RoleScrambleFrequency { get; } = new("NewMod.Options.GE.RoleScrambleFrequency", 0f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption ScreenDesynchronizationFrequency { get; } = new("NewMod.Options.GE.ScreenDesynchronizationFrequency", 15f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption SystemOverrideFrequency { get; } = new("NewMod.Options.GE.SystemOverrideFrequency", 15f, 0f, 100f, 5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedToggleOption ShowCrimsonWarning { get; } = new("NewMod.Options.GE.ShowCrimsonWarning", true) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption CrimsonDuration { get; } = new("NewMod.Options.GE.CrimsonDuration", 30f, 10f, 60f, 5f, MiraNumberSuffixes.Seconds) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedNumberOption CrimsonRadius { get; } = new("NewMod.Options.GE.CrimsonRadius", 5f, 2f, 20f, 0.5f, MiraNumberSuffixes.None) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedEnumOption<VortexIntensity> CrimsonIntensity { get; } = new("NewMod.Options.GE.CrimsonIntensity", VortexIntensity.Standard) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };

    public ModdedEnumOption<EscapeDifficulty> CrimsonEscapeDifficulty { get; } = new("NewMod.Options.GE.CrimsonEscapeDifficulty", EscapeDifficulty.Standard) { Visible = () => OptionGroupSingleton<GEOptions>.Instance.EnableGeneralEvents.Value };
}
