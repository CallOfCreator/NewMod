using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles.S1;

namespace NewMod.Options.Roles.S1;

[MiraIgnore]
public sealed class BountyOptions : AbstractOptionGroup<Bounty>
{
    public override string GroupName => "Bounty Settings";

    [ModdedNumberOption("Cash Out Cooldown", 10f, 30f, 5f, MiraNumberSuffixes.Seconds)]
    public float CashOutCooldown { get; set; } = 15f;

    [ModdedNumberOption("Cash Out Range", 0.75f, 2.5f, 0.25f)]
    public float CashOutRange { get; set; } = 1.5f;

    [ModdedNumberOption("Escort Range", 1f, 4f, 0.25f)]
    public float EscortRange { get; set; } = 2f;

    [ModdedNumberOption("Escort Duration", 10f, 45f, 5f, MiraNumberSuffixes.Seconds)]
    public float EscortDuration { get; set; } = 25f;

    [ModdedNumberOption("Collection Duration", 5f, 20f, 1f, MiraNumberSuffixes.Seconds)]
    public float CollectionDuration { get; set; } = 10f;
}