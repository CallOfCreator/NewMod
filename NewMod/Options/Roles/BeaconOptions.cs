using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Options.Roles
{
    public class BeaconOptions : AbstractOptionGroup<Beacon>
    {
        public override string GroupName => "Beacon Options";

        [ModdedNumberOption("Tasks per Charge", min: 1f, max: 6f, suffixType: MiraNumberSuffixes.None)]
        public float TasksPerCharge { get; set; } = 2f;

        [ModdedNumberOption("Max Charges", min: 1f, max: 6f, suffixType: MiraNumberSuffixes.None)]
        public float MaxCharges { get; set; } = 3f;

        [ModdedNumberOption("Pulse Duration", min: 1f, max: 30f, suffixType: MiraNumberSuffixes.Seconds)]
        public float PulseDuration { get; set; } = 15f;

        [ModdedNumberOption("Pulse Cooldown", min: 0f, max: 60f, suffixType: MiraNumberSuffixes.Seconds)]
        public float PulseCooldown { get; set; } = 15f;

        [ModdedToggleOption("Show Live Counts During Pulse")]
        public bool ShowOnMinimap { get; set; } = true;

        [ModdedToggleOption("Include Dead Bodies")]
        public bool IncludeDeadBodies { get; set; } = false;
    }
}
