using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Roles.NeutralRoles;

namespace NewMod.Options.Roles;

public class PranksterOptions : AbstractOptionGroup
{
    public override string GroupName => "Prankster";
    public override Type AdvancedRole => typeof(Prankster);

    [ModdedNumberOption("Prank Cooldown", min:10, max:40, suffixType:MiraNumberSuffixes.Seconds)]
    public float PrankCooldown {get; set;} = 20f;

    [ModdedNumberOption("Prank Max Uses", min:1, max:3)]
    public float PrankMaxUses {get; set;} = 2f;
}