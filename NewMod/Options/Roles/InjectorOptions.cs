using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class InjectorOptions : AbstractOptionGroup<InjectorRole>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.Injector");

    [ModdedNumberOption("NewMod.Options.Injector.SerumCooldown", 5f, 60f)]
    public float SerumCooldown { get; set; } = 20f;

    [ModdedNumberOption("NewMod.Options.Injector.RequiredInjectCount", 1, 10, 1, "#", "#")]
    public int RequiredInjectCount { get; set; } = 3;

    [ModdedNumberOption("NewMod.Options.Injector.InjectionRange", 0.5f, 2f, 0.25f, MiraNumberSuffixes.Distance)]
    public float InjectionRange { get; set; } = 1.25f;

    [ModdedNumberOption("NewMod.Options.Injector.ObservationDuration", 2f, 10f)]
    public float ObservationDuration { get; set; } = 5f;

    [ModdedNumberOption("NewMod.Options.Injector.CollectionWindow", 5f, 30f)]
    public float CollectionWindow { get; set; } = 15f;

    [ModdedNumberOption("NewMod.Options.Injector.AdrenalineSpeedBoost", 5f, 40f, 5f)]
    public float AdrenalineSpeedBoost { get; set; } = 20f;

    [ModdedNumberOption("NewMod.Options.Injector.SedativeSlowdown", 5f, 30f, 5f)]
    public float SedativeSlowdown { get; set; } = 15f;

    [ModdedNumberOption("NewMod.Options.Injector.CleanseDuration", 1f, 5f, 0.5f)]
    public float CleanseDuration { get; set; } = 2f;

    [ModdedNumberOption("NewMod.Options.Injector.SubmissionDuration", 3f, 15f)]
    public float SubmissionDuration { get; set; } = 6f;
}
