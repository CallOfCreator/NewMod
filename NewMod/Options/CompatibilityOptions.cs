using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;

namespace NewMod.Options;

#pragma warning disable
public class CompatibilityOptions : AbstractOptionGroup
{
    public override string GroupName => "Mod Compatibility";
    public override Func<bool> GroupVisible => ModCompatibility.IsLaunchpadLoaded;
    public ModdedToggleOption AllowRevenantHitmanCombo { get; } = new("Allow Revenant & Hitman in Same Match", false)
    {
        ChangedEvent = value =>
        {
            HudManager.Instance.ShowPopUp(value
                ? "You enabled the Revenant & Hitman combo. This may break game balance!"
                : "Revenant & Hitman combo disabled. Only one will be allowed per match.");
        }
    };
    public ModdedEnumOption<ModPriority> Compatibility { get; } = new("Mod Compatibility", ModPriority.PreferNewMod)
    {
        ChangedEvent = value =>
        {
            HudManager.Instance.ShowPopUp(
            value switch
            {
                ModPriority.PreferNewMod => "You selected 'PreferNewMod'. Medic will be disabled.\n" +
                                            "Switch to 'Prefer LaunchpadReloaded' to enable Medic and disable Necromancer.",
                ModPriority.PreferLaunchpadReloaded => "You selected 'PreferLaunchpadReloaded'. Necromancer will be disabled.\n" +
                                            "Switch to 'PreferNewMod' to enable Necromancer and disable Medic.",
            });
        }
    };
    public enum ModPriority
    {
        PreferNewMod,
        PreferLaunchpadReloaded
    }
}