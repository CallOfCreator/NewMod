using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;

namespace NewMod.Options.Roles.S1;

[MiraIgnore]
public sealed class DeadwireOptions : AbstractOptionGroup<global::NewMod.Roles.ImpostorRoles.S1.Deadwire>
{
    public override string GroupName => "Deadwire Settings";

    [ModdedNumberOption("Deadlock Cooldown", 25f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float DeadlockCooldown { get; set; } = 35f;

    [ModdedNumberOption("Deadlock Duration", 6f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float DeadlockDuration { get; set; } = 10f;

    [ModdedNumberOption("Deadlock Uses", 1f, 3f)]
    public float DeadlockUses { get; set; } = 2f;

    [ModdedNumberOption("Deadlock Range", 0.75f, 2.5f, 0.25f)]
    public float DeadlockRange { get; set; } = 1.5f;

    [ModdedNumberOption("Override Cooldown", 3f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float OverrideCooldown { get; set; } = 5f;

    [ModdedNumberOption("Tracking Duration", 3f, 12f, 1f, MiraNumberSuffixes.Seconds)]
    public float TrackingDuration { get; set; } = 6f;

    [ModdedNumberOption("Blink Distance", 1f, 4f, 0.25f)]
    public float BlinkDistance { get; set; } = 2.5f;

    [ModdedNumberOption("Jam Duration", 3f, 12f, 1f, MiraNumberSuffixes.Seconds)]
    public float JamDuration { get; set; } = 6f;

    [ModdedNumberOption("Barrier Duration", 3f, 12f, 1f, MiraNumberSuffixes.Seconds)]
    public float BarrierDuration { get; set; } = 6f;
}
