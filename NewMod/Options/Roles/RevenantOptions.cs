using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

public sealed class RevenantOptions : AbstractOptionGroup<Revenant>
{
    public override string GroupName => "Revenant Settings";

    [ModdedNumberOption("Feign Death Cooldown", 15f, 45f, 5f, MiraNumberSuffixes.Seconds)]
    public float FeignDeathCooldown { get; set; } = 25f;

    [ModdedNumberOption("Feign Duration", 5f, 12f, 1f, MiraNumberSuffixes.Seconds)]
    public float FeignDuration { get; set; } = 8f;

    [ModdedNumberOption("Doom Duration", 4f, 10f, 1f, MiraNumberSuffixes.Seconds)]
    public float DoomDuration { get; set; } = 7f;

    [ModdedNumberOption("Doom Speed Bonus", 10f, 40f, 5f, MiraNumberSuffixes.Percent)]
    public float DoomSpeedBonus { get; set; } = 25f;

    [ModdedNumberOption("Doom Contact Radius", 0.45f, 0.9f, 0.05f)]
    public float DoomContactRadius { get; set; } = 0.65f;
}