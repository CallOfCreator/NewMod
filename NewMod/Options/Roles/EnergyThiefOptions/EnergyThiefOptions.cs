using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles.EnergyThiefOptions;

public class EnergyThiefOptions : AbstractOptionGroup<EnergyThief>
{
    public override string GroupName => "Energy Thief";

    [ModdedNumberOption("Drain Cooldown", min: 10f, max: 20f, suffixType: MiraNumberSuffixes.Seconds)]
    public float DrainCooldown { get; set; } = 15f;

    [ModdedNumberOption("Drain Max Uses", min: 3f, max: 5f)]
    public float DrainMaxUses { get; set; } = 3f;

    [ModdedNumberOption("Required Drain Count", min: 2f, max: 4f)]
    public float RequiredDrainCount { get; set; } = 3f;
}