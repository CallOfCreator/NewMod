using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles.EgoistOptions
{
    public class EgoistRoleOptions : AbstractOptionGroup<EgoistRole>
    {
        public override string GroupName => "Egoist Settings";

        public ModdedNumberOption MinimumVotesToWin { get; } =
            new(
                "Minimum votes on Egoist to trigger win",
                3,
                min: 1,
                max: 10,
                increment: 1,
                suffixType: MiraNumberSuffixes.None
            );
    }
}
