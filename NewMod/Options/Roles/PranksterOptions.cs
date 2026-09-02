using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class PranksterOptions : AbstractOptionGroup<Prankster>
{
    public override string GroupName => "Prankster";

    [ModdedNumberOption("Prank Cooldown", 10, 40, suffixType: MiraNumberSuffixes.Seconds)]
    public float PrankCooldown { get; set; } = 20f;

    [ModdedNumberOption("Prank Max Uses", 1, 3)]
    public float PrankMaxUses { get; set; } = 2f;

    [ModdedNumberOption("Reports Required To Win", 1, 3)]
    public float ReportsRequiredToWin { get; set; } = 2f;
}
