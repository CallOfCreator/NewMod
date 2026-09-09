using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles.S1;

namespace NewMod.Options.Roles.S1;

[MiraIgnore]
public sealed class UsurperOptions : AbstractOptionGroup<Usurper>
{
    public override string GroupName => "Usurper Settings";

    [ModdedNumberOption("Claim Cooldown", 10f, 35f, 5f, MiraNumberSuffixes.Seconds)]
    public float ClaimCooldown { get; set; } = 20f;

    [ModdedNumberOption("Claim Range", 0.75f, 2.5f, 0.25f)]
    public float ClaimRange { get; set; } = 1.5f;

    [ModdedNumberOption("Crown Pickup Range", 0.5f, 2f, 0.25f)]
    public float CrownPickupRange { get; set; } = 1f;
}