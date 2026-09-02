using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles;

namespace NewMod.Options.Roles;

public sealed class DoubleAgentOptions : AbstractOptionGroup<DoubleAgent>
{
    public override string GroupName => "Double Agent Settings";

    [ModdedNumberOption("Counterfeit Duration", 3f, 8f, 1f, MiraNumberSuffixes.Seconds)]
    public float CounterfeitDuration { get; set; } = 5f;
}
