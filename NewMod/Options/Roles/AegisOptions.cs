using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Options.Roles;

public class AegisOptions : AbstractOptionGroup<Aegis>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.Aegis");

    [ModdedNumberOption("NewMod.Options.Aegis.AegisCooldown", 5f, 60f, suffixType: MiraNumberSuffixes.Seconds)]
    public float AegisCooldown { get; set; } = 25f;

    [ModdedNumberOption("NewMod.Options.Aegis.MaxCharges", 1f, 5f, suffixType: MiraNumberSuffixes.None)]
    public float MaxCharges { get; set; } = 2f;

    [ModdedNumberOption("NewMod.Options.Aegis.DurationSeconds", 2f, 30f, suffixType: MiraNumberSuffixes.Seconds)]
    public float DurationSeconds { get; set; } = 10f;

    [ModdedNumberOption("NewMod.Options.Aegis.Radius", 1f, 7f, suffixType: MiraNumberSuffixes.None)]
    public float Radius { get; set; } = 3f;
}
