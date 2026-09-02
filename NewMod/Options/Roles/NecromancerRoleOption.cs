using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

public class NecromancerOption : AbstractOptionGroup<NecromancerRole>
{
    public override string GroupName => "Necromancer Role";

    [ModdedNumberOption("Revive Cooldown", 15, 60, suffixType: MiraNumberSuffixes.Seconds)]
    public float ButtonCooldown { get; set; } = 30f;

    [ModdedNumberOption("Revive Uses", 1, 3)]
    public float AbilityUses { get; set; } = 1f;
}
