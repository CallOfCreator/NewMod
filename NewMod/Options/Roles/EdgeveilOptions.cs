using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

public class EdgeveilOptions : AbstractOptionGroup<Edgeveil>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.Edgeveil");

    [ModdedNumberOption("NewMod.Options.Edgeveil.SlashCooldown", 15f, 60f, 1f, MiraNumberSuffixes.Seconds)]
    public float SlashCooldown { get; set; } = 25f;

    [ModdedNumberOption("NewMod.Options.Edgeveil.SlashRange", 1f, 6f)]
    public float SlashRange { get; set; } = 2.5f;

    [ModdedNumberOption("NewMod.Options.Edgeveil.SlashSpeed", 1f, 6f)]
    public float SlashSpeed { get; set; } = 3f;

    [ModdedNumberOption("NewMod.Options.Edgeveil.ChargeDuration", 0.3f, 2f, 0.1f, MiraNumberSuffixes.Seconds)]
    public float ChargeDuration { get; set; } = 0.7f;

    [ModdedNumberOption("NewMod.Options.Edgeveil.PlayersToKill", 1f, 6f)]
    public float PlayersToKill { get; set; } = 1f;
}
