using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class EgoistRoleOptions : AbstractOptionGroup<EgoistRole>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.Egoist");

    public ModdedNumberOption EgoRequired { get; } = new("NewMod.Options.Egoist.EgoRequired", 4, 1, 10, 1, MiraNumberSuffixes.None);
}
