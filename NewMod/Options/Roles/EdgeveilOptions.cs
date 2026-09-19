using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

public class EdgeveilOptions : AbstractOptionGroup<Edgeveil>
{
    public override string GroupName => "Edgeveil Settings";

    [ModdedNumberOption("Slash Cooldown", 15f, 60f, 1f, MiraNumberSuffixes.Seconds)]
    public float SlashCooldown { get; set; } = 25f;

    [ModdedNumberOption("Slash Range", 1f, 6f)]
    public float SlashRange { get; set; } = 2.5f;

    [ModdedNumberOption("Slash Tray Speed", 1f, 6f)]
    public float SlashSpeed { get; set; } = 3f;

    [ModdedNumberOption("Charge Duration", 0.3f, 2f, 0.1f, MiraNumberSuffixes.Seconds)]
    public float ChargeDuration { get; set; } = 0.7f;

    [ModdedNumberOption("Max Players The Arc Can Kill", 1f, 6f)]
    public float PlayersToKill { get; set; } = 1f;
}