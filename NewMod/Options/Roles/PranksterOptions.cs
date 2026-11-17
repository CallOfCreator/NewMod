using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class PranksterOptions : AbstractOptionGroup<Prankster>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.Prankster");

    [ModdedNumberOption("NewMod.Options.Prankster.PrankCooldown", 10, 40, suffixType: MiraNumberSuffixes.Seconds)]
    public float PrankCooldown { get; set; } = 20f;

    [ModdedNumberOption("NewMod.Options.Prankster.InspectRange", 0.5f, 1.5f, 0.25f)]
    public float InspectRange { get; set; } = 0.75f;

    [ModdedNumberOption("NewMod.Options.Prankster.InspectDuration", 1f, 5f, 0.5f)]
    public float InspectDuration { get; set; } = 2f;

    [ModdedNumberOption("NewMod.Options.Prankster.ReportsRequiredToWin", 1, 3)]
    public float ReportsRequiredToWin { get; set; } = 2f;
}
