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

    [ModdedNumberOption("Body Inspection Range", 0.5f, 1.5f, 0.25f)]
    public float InspectRange { get; set; } = 0.75f;

    [ModdedNumberOption("Body Inspection Duration", 1f, 5f, 0.5f)]
    public float InspectDuration { get; set; } = 2f;

    [ModdedNumberOption("Reports Required To Win", 1, 3)]
    public float ReportsRequiredToWin { get; set; } = 2f;
}