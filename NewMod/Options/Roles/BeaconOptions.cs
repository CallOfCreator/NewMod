using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Options.Roles;

public class BeaconOptions : AbstractOptionGroup<Beacon>
{
    public override string GroupName => "Beacon Options";

    [ModdedNumberOption("Starting Charges", 0f, 3f, suffixType: MiraNumberSuffixes.None)]
    public float StartingCharges { get; set; } = 1f;

    [ModdedNumberOption("Tasks per Charge", 1f, 6f, suffixType: MiraNumberSuffixes.None)]
    public float TasksPerCharge { get; set; } = 3f;

    [ModdedNumberOption("Max Charges", 1f, 6f, suffixType: MiraNumberSuffixes.None)]
    public float MaxCharges { get; set; } = 2f;

    [ModdedNumberOption("Pulse Duration", 1f, 30f, suffixType: MiraNumberSuffixes.Seconds)]
    public float PulseDuration { get; set; } = 8f;

    [ModdedNumberOption("Pulse Cooldown", 0f, 60f, suffixType: MiraNumberSuffixes.Seconds)]
    public float PulseCooldown { get; set; } = 25f;

    [ModdedToggleOption("Show Live Counts During Pulse")]
    public bool ShowOnMinimap { get; set; } = true;

    [ModdedToggleOption("Include Dead Bodies")]
    public bool IncludeDeadBodies { get; set; } = false;
}
