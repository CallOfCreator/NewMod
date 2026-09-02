using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public sealed class EnergyThiefOptions : AbstractOptionGroup<EnergyThief>
{
    public override string GroupName => "Energy Thief";

    [ModdedNumberOption("Siphon Cooldown", 10f, 40f, 5f, MiraNumberSuffixes.Seconds)]
    public float SiphonCooldown { get; set; } = 20f;

    [ModdedNumberOption("Siphon Duration", 6f, 20f, 1f, MiraNumberSuffixes.Seconds)]
    public float SiphonDuration { get; set; } = 12f;

    [ModdedNumberOption("Siphon Range", 1f, 4f, 0.25f)]
    public float SiphonRange { get; set; } = 2.5f;

    [ModdedNumberOption("Brownout Duration", 3f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float BrownoutDuration { get; set; } = 6f;

    [ModdedNumberOption("Energy Required", 60f, 150f, 10f)]
    public float EnergyRequired { get; set; } = 100f;

    [ModdedNumberOption("Distinct Categories Required", 2f, 5f)]
    public float CategoriesRequired { get; set; } = 3f;

    [ModdedNumberOption("Grid Breach Duration", 6f, 20f, 1f, MiraNumberSuffixes.Seconds)]
    public float GridBreachDuration { get; set; } = 10f;

    [ModdedNumberOption("Grid Breach Radius", 1f, 3f, 0.25f)]
    public float GridBreachRadius { get; set; } = 1.5f;

    [ModdedNumberOption("Ground Duration", 1f, 4f, 0.25f, MiraNumberSuffixes.Seconds)]
    public float GroundDuration { get; set; } = 2f;

    [ModdedNumberOption("Energy Lost On Interruption", 10f, 50f, 5f)]
    public float EnergyLostOnInterruption { get; set; } = 25f;
}