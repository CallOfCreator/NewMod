using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

/// <summary>
///     Configurable options for the PulseBlade role.
/// </summary>
public class PulseBladeOptions : AbstractOptionGroup<PulseBlade>
{
    public override string GroupName => "PulseBlade Settings";

    [ModdedNumberOption("Strike Cooldown", 5, 60, suffixType: MiraNumberSuffixes.Seconds)]
    public float StrikeCooldown { get; set; } = 20f;

    [ModdedNumberOption("Max Strike Uses", 1, 5)]
    public float MaxStrikeUses { get; set; } = 3f;

    [ModdedNumberOption("Strike Range", 1f, 7f, 1f, MiraNumberSuffixes.None)]
    public float StrikeRange { get; set; } = 4f;

    [ModdedNumberOption("Dash Speed", 2f, 10f, 1f, MiraNumberSuffixes.None)]
    public float DashSpeed { get; set; } = 5f;

    [ModdedNumberOption("Hide Body Duration", 0f, 10f, 1f, MiraNumberSuffixes.Seconds)]
    public float HideBodyDuration { get; set; } = 5f;

    [ModdedNumberOption("Required Strikes to Win", 1f, 4f, 1f, MiraNumberSuffixes.None)]
    public float RequiredStrikes { get; set; } = 2f;

    [ModdedNumberOption("Players Remaining Threshold", 2f, 6f, 1f, MiraNumberSuffixes.None)]
    public float PlayersThreshold { get; set; } = 4f;
}