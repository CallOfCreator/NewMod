using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using NewMod.Roles.CrewmateRoles.S1;

namespace NewMod.Options.Roles.S1
{
    [MiraIgnore]
    public class VerifierOptions : AbstractOptionGroup<VerifierRole>
    {
        public override string GroupName => "Verifier Settings";

        [ModdedNumberOption("Unknown Result Chance", min: 0f, max: 70f, increment: 5f, suffixType: MiraNumberSuffixes.Percent)]
        public float UnknownChance { get; set; } = 25f;

        [ModdedNumberOption("Near Body Radius", min: 1f, max: 8f, increment: 0.5f, suffixType: MiraNumberSuffixes.None)]
        public float NearBodyRadius { get; set; } = 3f;
    }
}