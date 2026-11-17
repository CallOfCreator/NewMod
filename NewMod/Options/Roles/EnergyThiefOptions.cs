using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public sealed class EnergyThiefOptions : AbstractOptionGroup<EnergyThief>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.EnergyThief");

    [ModdedNumberOption("NewMod.Options.EnergyThief.SiphonCooldown", 10f, 40f, 5f, MiraNumberSuffixes.Seconds)]
    public float SiphonCooldown { get; set; } = 20f;

    [ModdedNumberOption("NewMod.Options.EnergyThief.SiphonDuration", 6f, 20f, 1f, MiraNumberSuffixes.Seconds)]
    public float SiphonDuration { get; set; } = 12f;

    [ModdedNumberOption("NewMod.Options.EnergyThief.SiphonRange", 1f, 4f, 0.25f)]
    public float SiphonRange { get; set; } = 2.5f;

    [ModdedNumberOption("NewMod.Options.EnergyThief.BrownoutDuration", 3f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float BrownoutDuration { get; set; } = 6f;

    [ModdedNumberOption("NewMod.Options.EnergyThief.EnergyRequired", 60f, 150f, 10f)]
    public float EnergyRequired { get; set; } = 100f;

    [ModdedNumberOption("NewMod.Options.EnergyThief.CategoriesRequired", 2f, 5f)]
    public float CategoriesRequired { get; set; } = 3f;

    [ModdedNumberOption("NewMod.Options.EnergyThief.GridBreachDuration", 6f, 20f, 1f, MiraNumberSuffixes.Seconds)]
    public float GridBreachDuration { get; set; } = 10f;

    [ModdedNumberOption("NewMod.Options.EnergyThief.GridBreachRadius", 1f, 3f, 0.25f)]
    public float GridBreachRadius { get; set; } = 1.5f;

    [ModdedNumberOption("NewMod.Options.EnergyThief.GroundDuration", 1f, 4f, 0.25f, MiraNumberSuffixes.Seconds)]
    public float GroundDuration { get; set; } = 2f;

    [ModdedNumberOption("NewMod.Options.EnergyThief.EnergyLostOnInterruption", 10f, 50f, 5f)]
    public float EnergyLostOnInterruption { get; set; } = 25f;
}
