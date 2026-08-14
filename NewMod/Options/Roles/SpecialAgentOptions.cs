using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class SpecialAgentOptions : AbstractOptionGroup<SpecialAgent>
{
    public override string GroupName => "Special Agent";

    [ModdedNumberOption("Assign Mission Cooldown", 10, 30)]
    public float AssignCooldown { get; set; } = 20f;

    [ModdedNumberOption("Assign Mission Max Uses", 1, 3)]
    public float AssignMaxUses { get; set; } = 3f;

    [ModdedToggleOption("Enable Target Camera Tracking")]
    public bool TargetCameraTracking { get; set; } = true;

    [ModdedNumberOption("Camera Tracking Duration", 5, 15)]
    public float CameraTrackingDuration { get; set; } = 10f;

    [ModdedNumberOption("Required Missions to Win", 1, 5)]
    public float RequiredMissionsToWin { get; set; } = 3f;

    [ModdedToggleOption("The camera should shake when the timer is close to the target")]
    public bool ShouldShakeCamera { get; set; } = false;
}