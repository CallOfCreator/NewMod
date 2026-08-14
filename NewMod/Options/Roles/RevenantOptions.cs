using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

public class RevenantOptions : AbstractOptionGroup<Revenant>
{
    public override string GroupName => "Revenant";

    [ModdedNumberOption("Feign Death Cooldown", 10, 40, suffixType: MiraNumberSuffixes.Seconds)]
    public float FeignDeathCooldown { get; set; } = 20f;

    [ModdedNumberOption("Feign Death Max Uses", 1, 3)]
    public float FeignDeathMaxUses { get; set; } = 2f;

    [ModdedNumberOption("Doom Awakening Cooldown", 10, 20, suffixType: MiraNumberSuffixes.Seconds)]
    public float DoomAwakeningCooldown { get; set; } = 10f;

    [ModdedNumberOption("Doom Awakening Max Uses", 1, 1)]
    public float DoomAwakeningMaxUses { get; set; } = 1f;

    [ModdedNumberOption("Doom Awakening Duration", 10f, 30f)]
    public float DoomAwakeningDuration { get; set; } = 20f;
}