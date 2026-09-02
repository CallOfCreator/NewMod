using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles.S1;

namespace NewMod.Options.Roles.S1;

[MiraIgnore]
public sealed class CollectorOptions : AbstractOptionGroup<Collector>
{
    public override string GroupName => "Collector Settings";

    [ModdedNumberOption("Harvest Cooldown", 5f, 20f, 5f, MiraNumberSuffixes.Seconds)]
    public float HarvestCooldown { get; set; } = 10f;

    [ModdedNumberOption("Harvest Range", 0.75f, 2.5f, 0.25f)]
    public float HarvestRange { get; set; } = 1.5f;

    [ModdedNumberOption("Harvest Reveal Range", 2f, 8f, 0.5f)]
    public float HarvestRevealRange { get; set; } = 5f;

    [ModdedNumberOption("Harvest Reveal Duration", 1f, 6f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float HarvestRevealDuration { get; set; } = 3f;

    [ModdedNumberOption("Trace Duration", 5f, 20f, 1f, MiraNumberSuffixes.Seconds)]
    public float TraceDuration { get; set; } = 12f;

    [ModdedNumberOption("Drift Duration", 4f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float DriftDuration { get; set; } = 8f;

    [ModdedNumberOption("Drift Speed Bonus", 5f, 30f, 5f, MiraNumberSuffixes.Percent)]
    public float DriftSpeedBonus { get; set; } = 15f;
}
