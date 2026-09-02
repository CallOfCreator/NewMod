using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles.S1;

namespace NewMod.Options.Roles.S1;

[MiraIgnore]
public class WardenOptions : AbstractOptionGroup<WardenRole>
{
    public override string GroupName => "Warden";

    [ModdedNumberOption("Seal Cooldown", 10f, 60f, suffixType: MiraNumberSuffixes.Seconds)]
    public float SealCooldown { get; set; } = 30f;

    [ModdedNumberOption("Seal Duration", 5f, 15f, suffixType: MiraNumberSuffixes.Seconds)]
    public float SealDuration { get; set; } = 8f;

    [ModdedNumberOption("Residual Mark Duration", 5f, 30f, suffixType: MiraNumberSuffixes.Seconds)]
    public float ResidualMarkDuration { get; set; } = 15f;
}