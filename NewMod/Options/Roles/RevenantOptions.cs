using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

public sealed class RevenantOptions : AbstractOptionGroup<Revenant>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.Revenant");

    [ModdedNumberOption("NewMod.Options.Revenant.FeignDeathCooldown", 15f, 45f, 5f, MiraNumberSuffixes.Seconds)]
    public float FeignDeathCooldown { get; set; } = 25f;

    [ModdedNumberOption("NewMod.Options.Revenant.FeignDuration", 5f, 12f, 1f, MiraNumberSuffixes.Seconds)]
    public float FeignDuration { get; set; } = 8f;

    [ModdedNumberOption("NewMod.Options.Revenant.DoomDuration", 4f, 10f, 1f, MiraNumberSuffixes.Seconds)]
    public float DoomDuration { get; set; } = 7f;

    [ModdedNumberOption("NewMod.Options.Revenant.DoomSpeedBonus", 10f, 40f, 5f, MiraNumberSuffixes.Percent)]
    public float DoomSpeedBonus { get; set; } = 25f;

    [ModdedNumberOption("NewMod.Options.Revenant.DoomContactRadius", 0.45f, 0.9f, 0.05f)]
    public float DoomContactRadius { get; set; } = 0.65f;
}
