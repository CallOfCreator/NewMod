using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class EnergyThiefOptions : AbstractOptionGroup<EnergyThief>
{
    public override string GroupName => "Energy Thief";

    [ModdedNumberOption("Drain Cooldown", 10f, 20f, suffixType: MiraNumberSuffixes.Seconds)]
    public float DrainCooldown { get; set; } = 15f;

    [ModdedNumberOption("Drain Max Uses", 3f, 5f)]
    public float DrainMaxUses { get; set; } = 3f;

    [ModdedNumberOption("Required Drain Count", 2f, 4f)]
    public float RequiredDrainCount { get; set; } = 3f;
}