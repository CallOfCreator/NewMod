using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

public class NecromancerOption : AbstractOptionGroup<NecromancerRole>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.Necromancer");

    [ModdedNumberOption("NewMod.Options.Necromancer.ReviveCooldown", 15, 60, suffixType: MiraNumberSuffixes.Seconds)]
    public float ButtonCooldown { get; set; } = 30f;

    [ModdedNumberOption("NewMod.Options.Necromancer.ReviveUses", 1, 3, 1, "#", "#")]
    public int AbilityUses { get; set; } = 1;
}
