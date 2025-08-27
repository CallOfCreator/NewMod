using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;

namespace NewMod.Options;
public class CompatibilityOptions : AbstractOptionGroup
{
    public override string GroupName => "Mod Compatibility";
    public override Func<bool> GroupVisible => ModCompatibility.IsLaunchpadLoaded;
    public ModdedToggleOption AllowRevenantHitmanCombo { get; } = new("Allow Revenant & Hitman in Same Match", false);
    public ModdedEnumOption<ModPriority> Compatibility { get; } = new("Mod Compatibility", ModPriority.PreferNewMod);
    public enum ModPriority
    {
        PreferNewMod,
        PreferLaunchpadReloaded
    }
}