using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class OverloadOptions : AbstractOptionGroup<OverloadRole>
{
    public override string GroupName => "The Overload";

    [ModdedNumberOption("Needed Charge", 1, 3)]
    public float NeededCharge { get; set; } = 3f;
}