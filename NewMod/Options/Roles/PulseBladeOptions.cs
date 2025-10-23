using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

/// <summary>
/// Configurable options for the PulseBlade role.
/// </summary>
public class PulseBladeOptions : AbstractOptionGroup<PulseBlade>
{
    public override string GroupName => "PulseBlade Settings";

    [ModdedNumberOption("Strike Cooldown", min: 5, max: 60, suffixType: MiraNumberSuffixes.Seconds)]
    public float StrikeCooldown { get; set; } = 20f;

    [ModdedNumberOption("Max Strike Uses", min: 1, max: 5)]
    public float MaxStrikeUses { get; set; } = 3f;

    [ModdedNumberOption("Strike Range", min: 1f, max: 7f, increment: 1f, suffixType: MiraNumberSuffixes.None)]
    public float StrikeRange { get; set; } = 4f;

    [ModdedNumberOption("Dash Speed", min: 2f, max: 10f, increment: 1f, suffixType: MiraNumberSuffixes.None)]
    public float DashSpeed { get; set; } = 5f;

    [ModdedNumberOption("Hide Body Duration", min: 0f, max: 10f, increment: 1f, suffixType: MiraNumberSuffixes.Seconds)]
    public float HideBodyDuration { get; set; } = 5f;

    [ModdedNumberOption("Required Strikes to Win", min: 1f, max: 4f, increment: 1f, suffixType: MiraNumberSuffixes.None)]
    public float RequiredStrikes { get; set; } = 2f;

    [ModdedNumberOption("Players Remaining Threshold", min: 2f, max: 6f, increment: 1f, suffixType: MiraNumberSuffixes.None)]
    public float PlayersThreshold { get; set; } = 4f;
}
