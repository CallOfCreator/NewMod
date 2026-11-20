using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Options.Roles;

public class BeaconOptions : AbstractOptionGroup<Beacon>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.Beacon");

    [ModdedNumberOption("NewMod.Options.Beacon.StartingCharges", 0f, 3f, suffixType: MiraNumberSuffixes.None)]
    public float StartingCharges { get; set; } = 1f;

    [ModdedNumberOption("NewMod.Options.Beacon.TasksPerCharge", 1f, 6f, suffixType: MiraNumberSuffixes.None)]
    public float TasksPerCharge { get; set; } = 3f;

    [ModdedNumberOption("NewMod.Options.Beacon.MaxCharges", 1f, 6f, suffixType: MiraNumberSuffixes.None)]
    public float MaxCharges { get; set; } = 2f;

    [ModdedNumberOption("NewMod.Options.Beacon.PulseDuration", 1f, 10f, suffixType: MiraNumberSuffixes.Seconds)]
    public float PulseDuration { get; set; } = 4f;

    [ModdedNumberOption("NewMod.Options.Beacon.PulseCooldown", 0f, 60f, suffixType: MiraNumberSuffixes.Seconds)]
    public float PulseCooldown { get; set; } = 25f;
}
