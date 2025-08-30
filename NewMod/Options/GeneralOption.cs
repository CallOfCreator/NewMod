using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace NewMod.Options;

public class GeneralOption : AbstractOptionGroup
{
    public override string GroupName => "NewMod Group";

    [ModdedToggleOption("Can Open Cams")]
    public bool CanOpenCams { get; set; } = true;

    [ModdedNumberOption("Total Neutrals", min:0f, max:10, 1f)]
    public float TotalNeutrals { get; set; } = 3f;

    [ModdedToggleOption("Keep Crew Majority")]
    public bool KeepCrewMajority { get; set; } = true;

    [ModdedToggleOption("Prefer Variety")]
    public bool PreferVariety { get; set; } = true;
}