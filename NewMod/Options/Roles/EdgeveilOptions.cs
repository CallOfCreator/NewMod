using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

public class EdgeveilOptions : AbstractOptionGroup<Edgeveil>
{
    public override string GroupName => "Edgeveil Settings";

    [ModdedNumberOption("Slash Cooldown", 15f, 60f, 1f, MiraNumberSuffixes.Seconds)]
    public float SlashCooldown { get; set; } = 20f;

    [ModdedNumberOption("Slash Max Uses", 1f, 3f, 1f, MiraNumberSuffixes.Seconds)]
    public float SlashMaxUses { get; set; } = 1f;

    [ModdedNumberOption("Slash Range", 1f, 6f, 1f, MiraNumberSuffixes.None)]
    public float SlashRange { get; set; } = 3f;

    [ModdedNumberOption("Slash Tray Speed", 1f, 6f, 1f, MiraNumberSuffixes.None)]
    public float SlashSpeed { get; set; } = 3f;

    [ModdedNumberOption("Duration of Shake Effect", 5f, 6f, 1f, MiraNumberSuffixes.None)]
    public float EffectDuration { get; set; } = 3f;

    [ModdedNumberOption("Max players the Arc can kill", 1f, 6f, 1f, MiraNumberSuffixes.None)]
    public float PlayersToKill { get; set; } = 4f;
}