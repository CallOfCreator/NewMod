using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

public class PulseBladeOptions : AbstractOptionGroup<PulseBlade>
{
    public override string GroupName => "PulseBlade Settings";

    [ModdedNumberOption("Strike Cooldown", 5, 60, suffixType: MiraNumberSuffixes.Seconds)]
    public float StrikeCooldown { get; set; } = 25f;

    [ModdedNumberOption("Strike Range", 1f, 7f)]
    public float StrikeRange { get; set; } = 2.5f;

    [ModdedNumberOption("Dash Speed", 2f, 10f)]
    public float DashSpeed { get; set; } = 6f;

    [ModdedNumberOption("Charge Duration", 0.3f, 2f, 0.1f, MiraNumberSuffixes.Seconds)]
    public float ChargeDuration { get; set; } = 0.7f;

    [ModdedNumberOption("Recovery Duration", 0.3f, 2f, 0.1f, MiraNumberSuffixes.Seconds)]
    public float RecoveryDuration { get; set; } = 0.8f;
}
