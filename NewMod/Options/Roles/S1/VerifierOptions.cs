using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles.S1;

namespace NewMod.Options.Roles.S1;

[MiraIgnore]
public class VerifierOptions : AbstractOptionGroup<VerifierRole>
{
    public override string GroupName => "Verifier Settings";

    [ModdedNumberOption("Near Body Radius", 1f, 8f, 0.5f)]
    public float NearBodyRadius { get; set; } = 3f;
}