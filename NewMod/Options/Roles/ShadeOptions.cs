using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class ShadeOptions : AbstractOptionGroup<Shade>
{
    public enum ShadowMode
    {
        Invisible,
        KillEnabled,
        Both
    }

    public override string GroupName => "Shade Options";

    [ModdedNumberOption("Shadow Cooldown", 5f, 60f, suffixType: MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 30f;

    [ModdedNumberOption("Max Shadow Uses", 1f, 5f, suffixType: MiraNumberSuffixes.None)]
    public float MaxUses { get; set; } = 2f;

    [ModdedNumberOption("Shadow Duration", 5f, 40f, suffixType: MiraNumberSuffixes.Seconds)]
    public float Duration { get; set; } = 12f;

    [ModdedNumberOption("Shadow Radius", 1f, 6f, suffixType: MiraNumberSuffixes.None)]
    public float Radius { get; set; } = 2.5f;

    [ModdedNumberOption("Required Kills To Win", 1f, 5f, suffixType: MiraNumberSuffixes.None)]
    public float RequiredKills { get; set; } = 3f;

    [ModdedEnumOption("Shadow Behavior", typeof(ShadowMode))]
    public ShadowMode Behavior { get; set; } = ShadowMode.Both;
}
