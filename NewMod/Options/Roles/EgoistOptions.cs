using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class EgoistRoleOptions : AbstractOptionGroup<EgoistRole>
{
    public override string GroupName => "Egoist Settings";

    public ModdedNumberOption EgoRequired { get; } = new("Votes required to unlock Challenge", 4, 1, 10, 1, MiraNumberSuffixes.None);
}