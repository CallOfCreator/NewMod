using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Options.Roles;

public class DoubleAgentOptions : AbstractOptionGroup<DoubleAgent>
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.DoubleAgent");

    [ModdedNumberOption("NewMod.Options.DoubleAgent.CounterfeitDuration", 3f, 8f, 1f, MiraNumberSuffixes.Seconds)]
    public float CounterfeitDuration { get; set; } = 5f;

    [ModdedNumberOption("NewMod.Options.DoubleAgent.CounterfeitCooldown", 20f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float CounterfeitCooldown { get; set; } = 30f;
}
