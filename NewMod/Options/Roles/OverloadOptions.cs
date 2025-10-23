using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class OverloadOptions : AbstractOptionGroup<OverloadRole>
{
    public override string GroupName => "The Overload";

    [ModdedNumberOption("Needed Charge", min:1, max:3)]
    public float NeededCharge { get; set; } = 2f;

    [ModdedNumberOption("Max Uses", min:1, max:2)]
    public float MaxUses { get; set; } = 1f;

}