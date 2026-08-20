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

    [ModdedNumberOption("Seal Cooldown", min: 10f, max: 60f, suffixType: MiraNumberSuffixes.Seconds)]
    public float SealCooldown { get; set; } = 30f;

    [ModdedNumberOption("Seal Duration", min: 5f, max: 15f, suffixType: MiraNumberSuffixes.Seconds)]
    public float SealDuration { get; set; } = 8f;

    [ModdedNumberOption("Residual Mark Duration", min: 5f, max: 30f, suffixType: MiraNumberSuffixes.Seconds)]
    public float ResidualMarkDuration { get; set; } = 15f;
}