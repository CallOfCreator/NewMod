using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;

namespace NewMod.Options;

public class GeneralOption : AbstractOptionGroup
{
    public override string GroupName => MiraLocaleManager.Get("NewMod.Options.Group.NewMod");

    [ModdedToggleOption("NewMod.Options.NewMod.AllowCams")]
    public bool AllowCams { get; set; } = false;

    [ModdedNumberOption("NewMod.Options.NewMod.TotalNeutrals", 0f, 10)]
    public float TotalNeutrals { get; set; } = 1f;

    [ModdedToggleOption("NewMod.Options.NewMod.KeepCrewMajority")]
    public bool KeepCrewMajority { get; set; } = true;

    [ModdedToggleOption("NewMod.Options.NewMod.PreferVariety")]
    public bool PreferVariety { get; set; } = true;

    [ModdedToggleOption("NewMod.Options.NewMod.ShouldDeadPlayersSeeRoles")]
    public bool ShouldDeadPlayersSeeRoles { get; set; } = true;

    [ModdedToggleOption("NewMod.Options.NewMod.EnableAnonymousNamesInMeetings")]
    public bool EnableAnonymousNamesInMeetings { get; set; } = false;
}
