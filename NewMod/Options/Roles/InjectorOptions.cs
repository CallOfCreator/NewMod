using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class InjectorOptions : AbstractOptionGroup<InjectorRole>
{
    public override string GroupName => "Injector Settings";

    [ModdedNumberOption("Serum Cooldown", 5f, 60f)]
    public float SerumCooldown { get; set; } = 20f;

    [ModdedNumberOption("Samples Required To Win", 1f, 10f)]
    public float RequiredInjectCount { get; set; } = 3f;

    [ModdedNumberOption("Injection And Collection Range", 0.5f, 2f, 0.25f)]
    public float InjectionRange { get; set; } = 1.25f;

    [ModdedNumberOption("Observation Duration", 2f, 10f)]
    public float ObservationDuration { get; set; } = 5f;

    [ModdedNumberOption("Sample Collection Window", 5f, 30f)]
    public float CollectionWindow { get; set; } = 15f;

    [ModdedNumberOption("Adrenaline Speed Bonus", 5f, 40f, 5f)]
    public float AdrenalineSpeedBoost { get; set; } = 20f;

    [ModdedNumberOption("Sedative Slowdown", 5f, 30f, 5f)]
    public float SedativeSlowdown { get; set; } = 15f;

    [ModdedNumberOption("Cleanse Duration", 1f, 5f, 0.5f)]
    public float CleanseDuration { get; set; } = 2f;

    [ModdedNumberOption("Final Submission Duration", 3f, 15f)]
    public float SubmissionDuration { get; set; } = 6f;
}