using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Roles;

namespace NewMod.Options.Roles;

public class EnergyThiefOptions : AbstractOptionGroup
{
    public override string GroupName => "Energy Thief";
    public override Type AdvancedRole => typeof(EnergyThief);

    [ModdedNumberOption("Drain Cooldown", min:10, max:20, suffixType:MiraNumberSuffixes.Seconds)]
    public float DrainCooldown {get; set;} = 15f;

    [ModdedNumberOption("Drain Max Uses", min:3, max:5)]
    public float DrainMaxUses {get; set;} = 3f;
}