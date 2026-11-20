using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class ShadeOptions : AbstractOptionGroup<Shade>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.Shade");

    [ModdedNumberOption("NewMod.Options.Shade.Cooldown", 5f, 60f, suffixType: MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 30f;

    [ModdedNumberOption("NewMod.Options.Shade.ActivationDelay", 0.5f, 4f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float ActivationDelay { get; set; } = 1.5f;

    [ModdedNumberOption("NewMod.Options.Shade.Duration", 5f, 40f, suffixType: MiraNumberSuffixes.Seconds)]
    public float Duration { get; set; } = 12f;

    [ModdedNumberOption("NewMod.Options.Shade.Radius", 1f, 6f, suffixType: MiraNumberSuffixes.None)]
    public float Radius { get; set; } = 2.5f;

    [ModdedNumberOption("NewMod.Options.Shade.RequiredKills", 1f, 5f, suffixType: MiraNumberSuffixes.None)]
    public float RequiredKills { get; set; } = 3f;
}
