using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.ImpostorRoles;

namespace NewMod.Options.Roles;

public class NecromancerOption : AbstractOptionGroup<NecromancerRole>
{
    public override string GroupName => "Necromancer Role";

    [ModdedNumberOption("ButtonCooldown", 5, 15, suffixType: MiraNumberSuffixes.Seconds)]
    public float ButtonCooldown { get; set; } = 6f;

    [ModdedNumberOption("AbilityUses", 1, 6)]
    public float AbilityUses { get; set; } = 3f;
}