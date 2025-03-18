using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles.RevenantOptions;

public class RevenantOptions : AbstractOptionGroup<Revenant>
{
    public override string GroupName => "Revenant";

    [ModdedNumberOption("Feign Death Cooldown", min: 10, max: 40, suffixType: MiraNumberSuffixes.Seconds)]
    public float FeignDeathCooldown { get; set; } = 20f;

    [ModdedNumberOption("Feign Death Max Uses", min: 1, max: 3)]
    public float FeignDeathMaxUses { get; set; } = 2f;

    [ModdedNumberOption("Doom Awakening Cooldown", min: 10, max: 20, suffixType: MiraNumberSuffixes.Seconds)]
    public float DoomAwakeningCooldown { get; set; } = 10f;

    [ModdedNumberOption("Doom Awakening Max Uses", min:1, max: 1)]
    public float DoomAwakeningMaxUses { get; set; } = 1f;

    [ModdedNumberOption("Doom Awakening Duration", min: 10f, max: 30f)]
    public float DoomAwakeningDuration { get; set; } = 20f;
}