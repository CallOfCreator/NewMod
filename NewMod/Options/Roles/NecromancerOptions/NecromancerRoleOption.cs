using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles.NecromancerOptions;

public class NecromancerOption : AbstractOptionGroup
{
      public override string GroupName => "Necromancer Role";
      public override Type AdvancedRole => typeof(NecromancerRole);

      [ModdedNumberOption("ButtonCooldown", min:5, max:15, suffixType:MiraNumberSuffixes.Seconds)] 
      public float ButtonCooldown {get; set;} = 6f;

      [ModdedNumberOption("AbilityUses", min:1, max:6)] 
      public float AbilityUses {get; set;} = 3f;
      
}